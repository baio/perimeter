namespace PRR.Domain.Auth.LogIn.Authorize

open PRR.Domain.Models

[<AutoOpen>]
module LogInUser =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open PRR.Data.DataContext
    open System
    open PRR.Domain.Auth.Common
    open Microsoft.Extensions.Logging
    open DataAvail.EntityFramework.Common
    open DataAvail.Http.Exceptions

    let private getClientAppData' (dataContext: DbDataContext) clientId =
        query {
            for app in dataContext.Applications do
                where (app.ClientId = clientId)

                select
                    (Tuple.Create
                        (app.SSOEnabled,
                         app.AllowedCallbackUrls,
                         Nullable(app.Domain.Pool.TenantId),
                         app.Domain.TenantId))
        }
        |> toSingleExnAsync (unAuthorized ("client_id not found"))

    let private getClientAppData (dataContext: DbDataContext) clientId =
        task {
            if clientId = PERIMETER_CLIENT_ID
            then return (true, "*", Nullable(), Nullable())
            else return! getClientAppData' dataContext clientId
        }

    type LoginData =
        { UserId: int
          ClientId: ClientId
          ResponseType: string
          State: string
          RedirectUri: string
          Scope: Scope
          Email: string
          CodeChallenge: string
          CodeChallengeMethod: string }

    let logInUser (env: Models.Env) (sso: Token option) (data: LoginData) (social: Social option) =

        let isPKCE = isNotEmpty data.CodeChallenge

        env.Logger.LogDebug
            ("LogIn user with ${@data} and ${sso} and social ${@social} and isPKCE ${isPKCE}",
             data,
             sso.IsSome,
             social,
             isPKCE)

        let dataContext = env.DataContext

        task {

            // TODO Validate data

            let! r = getAppInfo env.DataContext data.ClientId data.Email 1<minutes>

            env.Logger.LogInformation("App info found ${@info}", r)

            let issuer = r.Issuer

            let clientId = r.ClientId

            let! r = getClientAppData dataContext clientId

            env.Logger.LogInformation("App data found ${@data}", r)

            let (ssoEnabled, callbackUrls, poolTenantId, domainTenantId) = r

            let tenantId =
                match (poolTenantId.HasValue, domainTenantId.HasValue) with
                // Perimeter app : profile / create tenant
                | (false, false) -> PERIMETER_TENANT_ID
                // Domain app
                | (true, false) -> poolTenantId.Value
                // Tenant management app
                | (false, true) -> domainTenantId.Value
                | (true, true) ->
                    env.Logger.LogWarning
                        ("Both ${@poolTenantId}, ${@domainTenantId} defined this is unexpected",
                         poolTenantId,
                         domainTenantId)

                    raise (unexpected "Both poolTenantId, domainTenantId defined")

            env.Logger.LogInformation("TenantId found ${tenantId}", tenantId)

            if (callbackUrls <> "*"
                && (callbackUrls.Split(",")
                    |> Seq.map (fun x -> x.Trim())
                    |> Seq.contains data.RedirectUri
                    |> not)) then
                env.Logger.LogInformation("${callbackUrls} and ${redirectUri} mismatch", callbackUrls, data.RedirectUri)
                return! raise (unAuthorized "return_uri mismatch")

            let code = env.CodeGenerator()

            let result: AuthorizeResult =
                { RedirectUri = data.RedirectUri
                  State = data.State
                  Code = code }

            env.Logger.LogInformation("Login ${@result} ready", result)

            let codeExpiresAt =
                DateTime.UtcNow.AddMinutes(float env.CodeExpiresIn)

            let scopes = data.Scope.Split " "

            env.Logger.LogInformation("Validate ${@scopes} for @{email} and @{clientId} ", scopes, data.Email, clientId)

            let! validatedScopes = validateScopes dataContext data.Email clientId scopes

            env.Logger.LogInformation("${@validatedScopes} validated", validatedScopes)

            let userId = data.UserId

            let codeChallenge =
                if isPKCE then data.CodeChallenge else code

            let loginItem: LogInKV =
                { Code = code
                  ClientId = data.ClientId
                  Issuer = issuer
                  CodeChallenge = codeChallenge
                  RequestedScopes = scopes
                  ValidatedScopes = validatedScopes
                  UserId = userId
                  UserEmail = data.Email
                  ExpiresAt = codeExpiresAt
                  RedirectUri = data.RedirectUri
                  Social = social }

            let ssoExpiresAt =
                DateTime.UtcNow.AddMinutes(float env.SSOExpiresIn)

            let ssoItem =
                match ssoEnabled, sso with
                | (true, Some sso) ->
                    env.Logger.LogDebug("With SSO ${sso}", sso)

                    Some
                        ({ Code = sso
                           UserId = userId
                           // Used only in SSO login, user without tenant should not be able to sso login anyway
                           TenantId = tenantId
                           ExpiresAt = ssoExpiresAt
                           Email = data.Email
                           Social = social }: SSOKV)
                // TODO : Handle case SSO enabled but sso token not found
                | _ ->
                    env.Logger.LogDebug("No SSO ${ssoEnabled}", ssoEnabled)
                    None

            let successData = (loginItem, ssoItem)

            env.Logger.LogDebug("${@successData} is ready", successData)

            let env': OnSuccess.Env =
                { Logger = env.Logger
                  KeyValueStorage = env.KeyValueStorage }

            do! onSuccess env' successData

            env.Logger.LogInformation("Login user success")

            return result
        }
