namespace PRR.Domain.Auth.LogIn.TokenAuthorizationCode

open System.Threading.Tasks
open PRR.Data.Entities
open PRR.Domain.Common.Events
open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.DataContext
open System
open System.Text.RegularExpressions
open PRR.Domain.Auth.Common
open Microsoft.Extensions.Logging
open DataAvail.Http.Exceptions
open PRR.Domain.Auth.LogIn.Common

[<AutoOpen>]
module TokenAuthorizationCode =

    let private replace (pat: string) (rep: string) (str: string) = Regex.Replace(str, pat, rep)

    // cleanup sample  https://auth0.com/docs/flows/call-your-api-using-the-authorization-code-flow-with-pkce#javascript-sample
    let cleanupCodeChallenge =
        replace "\+" "-"
        >> replace "\/" "_"
        >> replace "=" ""

    let private validateData (isPKCE: bool) (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "code" data.Code)
           (validateNullOrEmpty "grant_type" data.Grant_Type)
           (validateContains [| "authorization_code" |] "grant_ype" data.Grant_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (if isPKCE
            then (validateNullOrEmpty "code_verifier" data.Code_Verifier)
            else (validateNullOrEmpty "client_secret" data.Client_Secret)) |]
        |> Array.choose id

    //
    let private checkPKCEFlow sha256Provider codeVerifier itemCodeChallenge =
        let codeChallenge =
            sha256Provider codeVerifier
            |> cleanupCodeChallenge

        if codeChallenge <> itemCodeChallenge
        then Some(unAuthorized "code_verifier code_challenge mismatch")
        else None


    //
    let private checkAuthorizationCode code itemCode =
        if code <> itemCode then Some(unAuthorized "code mismatch") else None


    let private checkAuthorizationCodeFlow (dbContext: DbDataContext) code itemCode clientId clientSecret =
        match checkAuthorizationCode code itemCode with
        | Some exn -> exn |> Some |> Task.FromResult
        | None -> checkApplicationClientSecret dbContext clientId clientSecret

    // https://auth0.com/docs/api/authentication?http#authorization-code-flow-with-pkce46
    // https://auth0.com/docs/api/authentication?http#authorization-code-flow45
    let tokenAuthorizationCode: TokenAuthorizationCode =
        fun env data ->

            env.Logger.LogInformation("LogInToken with ${@data}", data)

            task {

                // if client_secret is empty consider this PKCE flow
                let isPKCE = isEmpty data.Client_Secret

                env.Logger.LogDebug("PKCE flow is ${isPKCE}", isPKCE)

                let validationResult = validateData isPKCE data

                if Seq.length validationResult > 0 then
                    env.Logger.LogWarning("Validation error ${@data}", validationResult)
                    return raise (BadRequest validationResult)

                let! item = env.KeyValueStorage.GetValue<LogInKV> data.Code None

                let item =
                    match item with
                    | Ok item ->
                        env.Logger.LogInformation("LogIn item found ${@item}", { item with Code = "***" })
                        item
                    | Result.Error err ->
                        env.Logger.LogWarning("Couldn't find LogIn item ${code} with error ${@error}", data.Code, err)
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

                let! exn =
                    match isPKCE with
                    | true ->
                        checkPKCEFlow env.Sha256Provider data.Code_Verifier item.CodeChallenge
                        |> Task.FromResult
                    | false ->
                        checkAuthorizationCodeFlow
                            env.DataContext
                            data.Code
                            item.CodeChallenge
                            data.Client_Id
                            data.Client_Secret

                match exn with
                | Some exn ->
                    env.Logger.LogDebug("Check secret or code challenge fails with ${@exn}", exn)
                    return raise exn
                | None -> ()

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

                    let grantType =
                        match isPKCE with
                        | true -> GrantType.AuthorizationCodePKCE
                        | false -> GrantType.AuthorizationCode

                    let! (result, clientId, isPerimeterClient) =
                        signInUser
                            signInUserEnv
                            tokenData
                            data.Client_Id
                            (ValidatedScopes item.ValidatedScopes)
                            grantType

                    let refreshTokenItem: RefreshTokenKV =
                        { Token = result.refresh_token
                          ClientId = clientId
                          UserId = item.UserId
                          ExpiresAt = DateTime.UtcNow.AddMinutes(float env.RefreshTokenExpiresIn)
                          Scopes = item.RequestedScopes
                          IsPerimeterClient = isPerimeterClient
                          SocialType = socialType }

                    env.Logger.LogInformation("Success with refreshToken ${@refreshToken}", refreshTokenItem)

                    let env': OnLogInTokenSuccess.Env =
                        { DataContext = env.DataContext
                          PublishEndpoint = env.PublishEndpoint
                          Logger = env.Logger
                          KeyValueStorage = env.KeyValueStorage }

                    let loginItem: Item =
                        { Code = Some item.Code
                          ClientId = data.Client_Id
                          UserId = item.UserId
                          Social = None }

                    let userData =
                        { UserEmail = item.UserEmail
                          UserId = item.UserId }

                    let logInGrantType =
                        match isPKCE with
                        | true -> LogInGrantType.AuthorizationCodePKCE userData
                        | false -> LogInGrantType.AuthorizationCode userData

                    do! onLoginTokenSuccess env' logInGrantType loginItem refreshTokenItem isPerimeterClient

                    return result
                | None ->
                    env.Logger.LogWarning
                        ("$tokenData for ${userId} and ${socialType} is not found", item.UserId, socialType)

                    return raise (unAuthorized "user is not found")
            }
