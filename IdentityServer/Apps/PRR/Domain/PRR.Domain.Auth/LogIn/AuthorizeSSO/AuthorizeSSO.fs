namespace PRR.Domain.Auth.LogIn.AuthorizeSSO

open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Models

open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open System
open PRR.Domain.Auth.Common
open Microsoft.Extensions.Logging
open DataAvail.KeyValueStorage.Core
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

[<AutoOpen>]
module internal AuthorizeSSO =

    let authorizeSSO: AuthorizeSSO =
        fun env ssoToken data ->

            env.Logger.LogDebug("LogIn SSO with data {@data} and token {ssoToken}", data, ssoToken)

            let emailPasswordEmpty =
                (isEmpty data.Email) && (isEmpty data.Password)

            match validateAuthorizeData (not emailPasswordEmpty) data with
            | Some ex ->
                env.Logger.LogWarning("Data validation failed {@ex}", ex)
                raise ex
            | None -> ()

            let dataContext = env.DataContext

            task {

                let! ssoItem = env.KeyValueStorage.GetValue<SSOKV> ssoToken None

                let ssoItem =
                    match ssoItem with
                    | Ok ssoItem ->
                        env.Logger.LogDebug("SSO {@item} is found", ssoItem)
                        ssoItem
                    | Error err ->
                        env.Logger.LogWarning("SSO Item is not found for ${token} with error ${@err}", ssoToken, err)
                        raise (unAuthorized "sso not found")

                if ssoItem.ExpiresAt < DateTime.UtcNow then raise (unAuthorized "sso expired")

                let! appInfo = getAppInfo env.DataContext data.Client_Id ssoItem.Email 1<minutes>

                env.Logger.LogInformation("AppInfo found ${@appInfo}", appInfo)

                let! app =
                    query {
                        for app in dataContext.Applications do
                            where (app.ClientId = appInfo.ClientId)

                            select
                                (Tuple.Create
                                    (app.Domain.Pool.TenantId,
                                     app.Domain.TenantId,
                                     app.AllowedCallbackUrls,
                                     app.IsDomainManagement,
                                     app.SSOEnabled))
                    }
                    |> toSingleOptionAsync

                let app =
                    match app with
                    | Some app ->
                        env.Logger.LogInformation("Application ${@app} found for ${clientId}", app, appInfo.ClientId)
                        app
                    | None ->
                        env.Logger.LogWarning("Application data is not found for ${clientId}", appInfo.ClientId)
                        raise (unAuthorized ("client_id not found"))

                let (poolTenantId, managementDomainTenantId, callbackUrls, isDomainManagementClient, ssoEnabled) = app

                if not ssoEnabled then
                    env.Logger.LogWarning("SSO is disabled for this app")
                    return raise (unAuthorized "SSO Disabled")

                let tenantId =
                    if managementDomainTenantId.HasValue then managementDomainTenantId.Value else poolTenantId

                if tenantId <> ssoItem.TenantId then

                    env.Logger.LogInformation
                        ("${tenantId} is not equal to ${ssoItemTenantId}", tenantId, ssoItem.TenantId)

                    // Switch to management client (from any tenant) should be valid
                    // admin should be relogined silently when switching between tenants
                    // TODO : Add IsTenantManagement to app, anyway we can figure it out implicitly like so

                    let isTenantManagementClient = managementDomainTenantId.HasValue

                    if not
                        (isDomainManagementClient
                         || isTenantManagementClient) then
                        env.Logger.LogWarning
                            ("Application tenant and tenant from sso item are different and ${isDomainManagementClient} or ${isTenantManagementClient} are not true, so this is common client, we dont allow switch tenant silently for these",
                             tenantId,
                             ssoItem.TenantId)

                        return raise (unAuthorized "sso wrong tenant")
                    else
                        env.Logger.LogInformation
                            ("${isDomainManagementClient} or ${isTenantManagementClient} is true, so this is admin client, we allow switch tenant silently for these",
                             tenantId,
                             ssoItem.TenantId)

                if (callbackUrls <> "*"
                    && (callbackUrls.Split(",")
                        |> Seq.map (fun x -> x.Trim())
                        |> Seq.contains data.Redirect_Uri
                        |> not)) then
                    env.Logger.LogWarning
                        ("${@dataRedirectUri} is not contained in ${@callbackUrls}", data.Redirect_Uri, callbackUrls)

                    return! raise (unAuthorized "return_uri mismatch")

                match! getUserId dataContext ssoItem.Email with
                | Some userId ->
                    env.Logger.LogInformation("${@userId} is found for ${@ssoItemEmail}", userId, ssoItem.Email)

                    let code = env.CodeGenerator()

                    let result =
                        sprintf "%s?code=%s&state=%s" data.Redirect_Uri code data.State
                    
                    env.Logger.LogInformation("${@result} is ready", result)

                    let expiresAt =
                        DateTime.UtcNow.AddMinutes(float env.CodeExpiresIn)

                    let scopes = data.Scope.Split " "

                    let! validatedScopes = validateScopes dataContext ssoItem.Email appInfo.ClientId scopes

                    let loginItem: LogInKV =
                        { Code = code
                          ClientId = data.Client_Id
                          Issuer = appInfo.Issuer
                          CodeChallenge = data.Code_Challenge
                          RequestedScopes = scopes
                          ValidatedScopes = validatedScopes
                          UserId = userId
                          UserEmail = ssoItem.Email
                          ExpiresAt = expiresAt
                          RedirectUri = data.Redirect_Uri
                          State = data.State
                          Nonce = data.Nonce
                          Social = None }

                    env.Logger.LogInformation("${@loginItem} ready", loginItem)

                    let options =
                        { Tag = userId.ToString()
                          ExpiresAt = (Some expiresAt)
                          PartitionName = null }

                    let! result' = env.KeyValueStorage.AddValue code loginItem (Some options)

                    match result' with
                    | Result.Error AddValueError.KeyAlreadyExists ->
                        env.Logger.LogError("Code ${code} already exists in LogIn storage", code)
                        return raise (Unexpected')
                    | _ -> ()

                    return result

                | None ->
                    env.Logger.LogWarning("userId is not found for ${email}", ssoItem.Email)
                    return! raise (unAuthorized "Wrong email or password")
            }
