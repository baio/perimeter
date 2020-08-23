namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models

open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Domain.Auth.LogInToken

[<AutoOpen>]
module internal SignInUser =

    // https://www.blinkingcaret.com/2018/05/30/refresh-tokens-in-asp-net-core-web-api/
    // https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api
    // https://jasonwatmore.com/post/2020/05/25/aspnet-core-3-api-jwt-authentication-with-refresh-tokens

    let signInUser' env clientId audiences tokenData rolesPermissions =

        let accessToken =
            createAccessTokenClaims clientId tokenData rolesPermissions audiences
            |> (createToken env.JwtConfig.AccessTokenSecret env.JwtConfig.AccessTokenExpiresIn)

        let idToken =
            createIdTokenClaims tokenData rolesPermissions
            |> (createToken env.JwtConfig.IdTokenSecret env.JwtConfig.IdTokenExpiresIn)

        let refreshToken = env.HashProvider()

        { IdToken = idToken
          AccessToken = accessToken
          RefreshToken = refreshToken }

    let signInUser env (tokenData: TokenData) clientId =
        task {
            let! clientId = PRR.Domain.Auth.LogIn.UserHelpers.getClientId env.DataContext clientId tokenData.Email
            match! getClientDomainAudiences env.DataContext clientId with
            | Some { DomainId = domainId; Audiences = audiences } ->
                let! userRolePemissions = getUserDomainRolesPermissions env.DataContext (domainId, tokenData.Email)
                let result = signInUser' env clientId audiences tokenData userRolePemissions
                return (result, clientId)
            | None ->
                return raise (unAuthorized "Client is not found")
        }
