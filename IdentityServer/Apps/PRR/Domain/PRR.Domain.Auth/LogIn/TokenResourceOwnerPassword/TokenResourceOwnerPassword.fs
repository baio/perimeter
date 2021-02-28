namespace PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword

open PRR.Data.Entities
open PRR.Domain.Common.Events

[<AutoOpen>]
module TokenResourceOwnerPassword =

    open System
    open DataAvail.Common
    open PRR.Data.DataContext
    open PRR.Domain.Auth.LogIn.Common
    open DataAvail.Http.Exceptions
    open Microsoft.Extensions.Logging
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open DataAvail.EntityFramework.Common.LinqHelpers
    open PRR.Domain.Auth.Common
    open PRR.Domain.Auth


    let private validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "grant_type" data.Grant_Type)
           (validateContains [| "password" |] "grant_ype" data.Grant_Type)
           (validateNullOrEmpty "username" data.Username)
           (validateNullOrEmpty "password" data.Password)
           (validateNullOrEmpty "scope" data.Scope) |]
        |> Array.choose id

    let private findUserId (dataContext: DbDataContext) (username: string) (hashedPassword: string) =
        query {
            for user in dataContext.Users do
                where
                    (user.Email = username
                     && user.Password = hashedPassword)

                select user.Id
        }
        |> toSingleOptionAsync

    // https://auth0.com/docs/api/authentication?http#resource-owner-password
    let tokenResourceOwnerPassword: TokenResourceOwnerPassword =
        fun env data ->

            let logger = env.Logger

            logger.LogDebug("TokenResourceOwnerPassword with ${@data}", data)

            task {
                let validationResult = validateData data

                if Seq.length validationResult > 0 then
                    logger.LogWarning("Validation error ${@data}", validationResult)
                    raise (BadRequest validationResult)

                let hashedPassword = env.StringSalter data.Password

                let! isValidCredentials = findUserId env.DataContext data.Username hashedPassword

                let userId =
                    match isValidCredentials with
                    | Some userId -> userId
                    | None ->
                        logger.LogWarning("Invalid username or password")
                        raise (unAuthorized "Invalid username or password")


                let! userDataForToken = getUserDataForToken env.DataContext userId None

                match userDataForToken with
                | None ->
                    env.Logger.LogError("User data is not found for ${userId}", userId)
                    return raise (unAuthorized "User data is not found")
                | Some tokenData ->

                    env.Logger.LogInformation("Token data {@tokenData} for {userId}", tokenData, userId)

                    let signInUserEnv: SignInUserEnv =
                        { DataContext = env.DataContext
                          JwtConfig = env.JwtConfig
                          Logger = env.Logger
                          HashProvider = env.HashProvider }

                    let scopes = data.Scope.Split " "

                    let! (result, clientId, isPerimeterClient) =
                        signInUser signInUserEnv tokenData data.Client_Id (RequestedScopes scopes) GrantType.Password

                    let refreshTokenItem =
                        match result.refresh_token with
                        | null -> None
                        | _ ->
                            Some
                                { Token = result.refresh_token
                                  ClientId = clientId
                                  UserId = userId
                                  ExpiresAt = DateTime.UtcNow.AddMinutes(float env.RefreshTokenExpiresIn)
                                  Scopes = scopes
                                  IsPerimeterClient = isPerimeterClient
                                  SocialType = None }

                    env.Logger.LogDebug("Success with refreshToken ${@refreshToken}", refreshTokenItem)

                    let env': OnLogInTokenSuccess.Env =
                        { DataContext = env.DataContext
                          PublishEndpoint = env.PublishEndpoint
                          Logger = env.Logger
                          KeyValueStorage = env.KeyValueStorage }

                    let loginItem: Item =
                        { Code = None
                          ClientId = data.Client_Id
                          UserId = userId
                          Social = None }

                    let grantType =
                        LogInGrantType.Password
                            { UserEmail = data.Username
                              UserId = userId }

                    do! onLoginTokenSuccess env' clientId grantType loginItem refreshTokenItem isPerimeterClient

                    return result
            }
