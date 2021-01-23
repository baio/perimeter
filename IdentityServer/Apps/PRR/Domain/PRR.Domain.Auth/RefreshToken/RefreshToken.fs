namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.Entities
open PRR.Domain.Auth.LogInToken
open PRR.System.Models
open PRR.Domain.Auth.Common

[<AutoOpen>]
module RefreshToken =

    let private getUserDataForToken dataContext userId =
        getUserDataForToken' dataContext <@ fun (user: User) -> (user.Id = userId) @>

    let getTokenIssuer =
        getTokenIssuer
        >> function
        | Some token -> token
        | None -> raise (unAuthorized "Token issuer not found")

    let refreshToken: RefreshToken =
        fun env token data ->
            let env': SignInUserEnv =
                { DataContext = env.DataContext
                  JwtConfig = env.JwtConfig
                  Logger = env.Logger
                  HashProvider = env.HashProvider }

            let accessToken = token

            task {
                let! item = env.GetTokenItem data.RefreshToken

                let item =
                    match item with
                    | Some item -> item
                    | None -> raise UnAuthorized'

                let issuer = getTokenIssuer accessToken
                let! domainSecret = getDomainSecretAndExpire env' issuer item.IsPerimeterClient

                match validate env accessToken (item.ExpiresAt, item.UserId, domainSecret.SigningCredentials.Key) with
                | Success ->
                    match! getUserDataForToken env.DataContext item.UserId item.SocialType with
                    | Some tokenData ->
                        // TODO : When available scopes changed while refreshing tokens what to do ?
                        // Now just silently change scopes
                        let scopes = item.Scopes

                        let! (res, clientId, _) = signInUser env' tokenData item.ClientId (RequestedScopes scopes)

                        do!
                            env.OnSuccess
                                { ClientId = clientId
                                  IsPerimeterClient = item.IsPerimeterClient
                                  UserId = tokenData.Id
                                  RefreshToken = res.refresh_token
                                  OldRefreshToken = item.Token
                                  Scopes = item.Scopes
                                  SocialType = item.SocialType }

                        return res
                    | None -> return! raise (UnAuthorized None)
                | _ -> return! raise (UnAuthorized None)
            }
