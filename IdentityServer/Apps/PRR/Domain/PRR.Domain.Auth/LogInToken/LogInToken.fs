namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Common.Domain.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Models
open PRR.Data.DataContext
open PRR.System.Models
open System
open System.Text.RegularExpressions
open PRR.Domain.Auth
open PRR.Domain.Auth.Common
open Microsoft.Extensions.Logging

[<AutoOpen>]
module LogInToken =

    let private replace (pat: string) (rep: string) (str: string) = Regex.Replace(str, pat, rep)

    // cleanup sample  https://auth0.com/docs/flows/call-your-api-using-the-authorization-code-flow-with-pkce#javascript-sample
    let cleanupCodeChallenge =
        replace "\+" "-"
        >> replace "\/" "_"
        >> replace "=" ""

    let private validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "code" data.Code)
           (validateNullOrEmpty "grant_type" data.Grant_Type)
           (validateContains [| "code" |] "grant_ype" data.Grant_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNullOrEmpty "code_verifier" data.Code_Verifier) |]
        |> Array.choose id

    let private getSuccessData (dataContext: DbDataContext) clientId userId isPerimeterClient =
        task {

            let! userEmail =
                query {
                    for user in dataContext.Users do
                        where (user.Id = userId)
                        select user.Email
                }
                |> toSingleExnAsync (Unexpected')

            if clientId = PERIMETER_CLIENT_ID then
                let result: LogIn.LoginSuccessData =
                    { DomainId = PERIMETER_DOMAIN_ID
                      IsManagementClient = true
                      AppIdentifier = PERIMETER_APP_IDENTIFIER
                      UserEmail = userEmail
                      Date = DateTime.UtcNow }

                return result
            else
                let! (domainId, appIdentifier) =
                    query {
                        for app in dataContext.Applications do
                            where (app.ClientId = clientId)
                            select (app.Domain.Id, app.Name)
                    }
                    |> toSingleExnAsync (Unexpected')

                let successData: LogIn.LoginSuccessData =
                    { DomainId = domainId
                      IsManagementClient = isPerimeterClient
                      AppIdentifier = appIdentifier
                      UserEmail = userEmail
                      Date = DateTime.UtcNow }

                return successData
        }


    let logInToken: LogInToken =
        fun env data ->

            env.Logger.LogInformation("LogInToken with ${@data}", { data with Code = "***" })

            task {

                let! item = env.GetCodeItem data.Code

                let item =
                    match item with
                    | Some item ->
                        env.Logger.LogInformation("Signup item found ${@item}", { item with Code = "***" })
                        item
                    | None ->
                        env.Logger.LogWarning("Couldn't find signup item ${code}", data.Code)
                        raise (UnAuthorized None)

                let dataContext = env.DataContext

                if (item.ExpiresAt < DateTime.UtcNow) then
                    env.Logger.LogWarning
                        ("LoginItem item expired at ${expiresAt} for code ${code}", item.ExpiresAt, data.Code)
                    raise (unAuthorized "code expires")

                if data.Client_Id <> item.ClientId then
                    env.Logger.LogWarning
                        ("${clientId} and ${itemClientId} mismatch error", data.Client_Id, item.ClientId)
                    raise (unAuthorized "client_id mismatch")

                if data.Redirect_Uri <> item.RedirectUri then
                    env.Logger.LogWarning
                        ("${redirectUri} and ${itemRedirectUri} mismatch error", data.Redirect_Uri, item.RedirectUri)
                    raise (unAuthorized "redirect_uri mismatch")

                let codeChallenge =
                    env.Sha256Provider(data.Code_Verifier)
                    |> cleanupCodeChallenge

                let itemCodeChallenge = item.CodeChallenge

                if codeChallenge <> itemCodeChallenge then
                    env.Logger.LogWarning
                        ("${codeChallenge} and ${itemCodeChallenge} mismatch error", codeChallenge, itemCodeChallenge)
                    raise (unAuthorized "code_verifier code_challenge mismatch")

                let socialType =
                    item.Social |> Option.map (fun f -> f.Type)

                match! getUserDataForToken dataContext item.UserId socialType with
                | Some tokenData ->
                    env.Logger.LogInformation
                        ("${@tokenData} for ${userId} and ${socialType} is found", tokenData, item.UserId, socialType)

                    let signInUserEnv: SignInUserEnv =
                        { DataContext = env.DataContext
                          JwtConfig = env.JwtConfig
                          Logger = env.Logger
                          HashProvider = env.HashProvider }

                    let! (result, clientId, isPerimeterClient) =
                        signInUser signInUserEnv tokenData data.Client_Id (ValidatedScopes item.ValidatedScopes)

                    let refreshTokenItem: RefreshToken.Item =
                        { Token = result.refresh_token
                          ClientId = clientId
                          UserId = item.UserId
                          ExpiresAt = DateTime.UtcNow.AddMinutes(float env.SSOCookieExpiresIn)
                          Scopes = item.RequestedScopes
                          IsPerimeterClient = isPerimeterClient
                          SocialType = socialType }

                    let! successData = getSuccessData dataContext clientId item.UserId isPerimeterClient

                    env.Logger.LogInformation("Success with ${@successData}", successData)

                    do! env.OnSuccess(item.Code, refreshTokenItem, successData)

                    return result
                | None ->
                    env.Logger.LogWarning
                        ("$tokenData for ${userId} and ${socialType} is not found", item.UserId, socialType)
                    return! raiseTask (unAuthorized "user is not found")
            }
