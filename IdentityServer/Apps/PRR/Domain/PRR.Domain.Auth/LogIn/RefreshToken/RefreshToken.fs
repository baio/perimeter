﻿namespace PRR.Domain.Auth.LogIn.RefreshToken

open System
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.Entities
open PRR.Domain.Auth.Common
open Microsoft.Extensions.Logging
open DataAvail.Http.Exceptions
open PRR.Domain.Auth.LogIn.Common

[<AutoOpen>]
module RefreshToken =

    let private getUserDataForToken dataContext userId =
        getUserDataForToken' dataContext <@ fun (user: User) -> (user.Id = userId) @>

    let getTokenIssuer =
        getTokenIssuer
        >> function
        | Some token -> token
        | None -> raise (unAuthorized "Token issuer not found")

    let private validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "refresh_token" data.Refresh_Token)
           (validateNullOrEmpty "grant_type" data.Grant_Type)
           (validateContains [| "refresh_token" |] "grant_ype" data.Grant_Type) |]
        |> Array.choose id

    // https://auth0.com/docs/api/authentication?http#refresh-token
    let refreshToken: RefreshToken =
        fun env token data ->

            let logger = env.Logger

            let env': SignInUserEnv =
                { DataContext = env.DataContext
                  JwtConfig = env.JwtConfig
                  Logger = env.Logger
                  HashProvider = env.HashProvider }

            let accessToken = token

            logger.LogDebug("refreshToken begins with access token ${accessToken}", accessToken)

            let validationResult = validateData data

            if Seq.length validationResult > 0 then
                logger.LogWarning("Validation error ${@data}", validationResult)
                raise (BadRequest validationResult)

            task {
                let! item = env.KeyValueStorage.GetValue<RefreshTokenKV> data.Refresh_Token None

                let item =
                    match item with
                    | Ok item ->
                        logger.LogInformation("${@item} is found for refresh token", item)
                        item
                    | Error err ->
                        logger.LogWarning
                            ("Refresh token item is not found for refresh token {token} with error ${@error}",
                             data.Refresh_Token,
                             err)

                        raise UnAuthorized'

                let issuer = getTokenIssuer accessToken

                logger.LogInformation("${@issuer} found for bearer token", issuer)

                let env'': GetDomainSecretAndExpireEnv =
                    { JwtConfig = env.JwtConfig
                      DataContext = env.DataContext }

                let! domainSecret = getDomainSecretAndExpire env'' issuer item.IsPerimeterClient

                match validate env accessToken (item.ExpiresAt, item.UserId, domainSecret.SigningCredentials.Key) with
                | Success ->
                    logger.LogInformation("Domain and token validated for refresh token item")

                    match! getUserDataForToken env.DataContext item.UserId item.SocialType with
                    | Some tokenData ->
                        logger.LogInformation("${@tokenData} for token found", tokenData)
                        // TODO : When available scopes changed while refreshing tokens what to do ?
                        // Now just silently change scopes
                        let scopes = item.Scopes

                        let! (res, clientId, _, issuer) =
                            signInUser env' tokenData item.ClientId null (RequestedScopes scopes) GrantType.RefreshToken

                        let newRefreshTokenItem: RefreshTokenKV =
                            { ClientId = clientId
                              IsPerimeterClient = item.IsPerimeterClient
                              UserId = tokenData.Id
                              Token = res.refresh_token
                              ExpiresAt = DateTime.UtcNow.AddMinutes(float env.TokenExpiresIn)
                              Scopes = item.Scopes
                              SocialType = item.SocialType }

                        logger.LogDebug("successData ${@successData} ready", newRefreshTokenItem)

                        do! onSuccess env issuer item.Token newRefreshTokenItem

                        return res
                    | None ->
                        logger.LogWarning("tokenData for token is not found")
                        return! raise (UnAuthorized None)
                | res ->
                    logger.LogWarning("${@error} while validating token", res)
                    return! raise (UnAuthorized None)
            }
