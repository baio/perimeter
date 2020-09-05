namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models

open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.DataContext
open PRR.Domain.Auth.LogInToken

[<AutoOpen>]
module internal SignInUser =

    // https://www.blinkingcaret.com/2018/05/30/refresh-tokens-in-asp-net-core-web-api/
    // https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api
    // https://jasonwatmore.com/post/2020/05/25/aspnet-core-3-api-jwt-authentication-with-refresh-tokens

    let signInUser' env clientId issuer tokenData (validatedScopes: AudienceScopes seq) =

        let audiences =
            validatedScopes |> Seq.map (fun x -> x.Audience)

        let rolesPermissions =
            validatedScopes |> Seq.collect (fun x -> x.Scopes)

        let accessToken =
            createAccessTokenClaims clientId issuer tokenData rolesPermissions audiences
            |> (createToken env.JwtConfig.AccessTokenSecret env.JwtConfig.AccessTokenExpiresIn)

        let idToken =
            createIdTokenClaims clientId issuer tokenData rolesPermissions
            |> (createToken env.JwtConfig.IdTokenSecret env.JwtConfig.IdTokenExpiresIn)

        let refreshToken = env.HashProvider()

        { id_token = idToken
          access_token = accessToken
          refresh_token = refreshToken }

    let private getClientAudiencesRolePermissions' (dataContext: DbDataContext) clientId email =
        task {
            let! domainAudiences = getClientDomainAudiences dataContext clientId

            match domainAudiences with
            | Some { DomainId = domainId; Audiences = audiences } ->
                let! rolesPermissions = getUserDomainRolesPermissions dataContext (domainId, email)
                return (audiences, (rolesPermissions :> seq<_>))
            | None -> return raise (unAuthorized "Client is not found")
        }


    let private perimeterUserRolePermissions =
        [| { Role = "PerimeterUser"
             Permissions = [ "profile"; "email" ] } |]
        |> Array.toSeq

    let private perimeterAudiences =
        [| PRR.Domain.Auth.Constants.PERIMETER_USERS_AUDIENCE |]
        |> Array.toSeq

    let private getClientAudiencesRolePermissions (dataContext: DbDataContext) clientId email =

        task {
            if clientId = PRR.Domain.Auth.Constants.PERIMETER_CLIENT_ID then
                return (perimeterAudiences, perimeterUserRolePermissions)
            else
                let! (audiences, rolesPermissions) = getClientAudiencesRolePermissions' dataContext clientId email

                return (Seq.append audiences perimeterAudiences),
                       (Seq.append rolesPermissions perimeterUserRolePermissions)
        }

    let signInUser env (tokenData: TokenData) clientId (validatedScopes: AudienceScopes seq) =
        task {
            let! (clientId, issuer) =
                PRR.Domain.Auth.LogIn.UserHelpers.getClientIdAndIssuer env.DataContext clientId tokenData.Email

            let result =
                signInUser' env clientId issuer tokenData validatedScopes

            return (result, clientId)
        }
