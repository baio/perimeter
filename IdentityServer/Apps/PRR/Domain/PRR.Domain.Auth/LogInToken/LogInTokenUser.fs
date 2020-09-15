namespace PRR.Domain.Auth.LogInToken

open System.Threading.Tasks
open Common.Domain.Models

open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.DataContext
open PRR.Domain.Auth
open PRR.Domain.Auth.LogInToken
open Common.Domain.Utils.LinqHelpers

[<AutoOpen>]
module internal SignInUser =

    // https://www.blinkingcaret.com/2018/05/30/refresh-tokens-in-asp-net-core-web-api/
    // https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api
    // https://jasonwatmore.com/post/2020/05/25/aspnet-core-3-api-jwt-authentication-with-refresh-tokens

    type SignInData =
        { TokenData: TokenData
          ClientId: ClientId
          Issuer: string
          AudienceScopes: AudienceScopes seq
          RefreshTokenProvider: unit -> string
          AccessTokenSecret: string
          AccessTokenExpiresIn: int<minutes>
          IdTokenExpiresIn: int<minutes> }

    let signInUser' (data: SignInData) =
        
        let audiences =
            data.AudienceScopes
            |> Seq.map (fun x -> x.Audience)

        let rolesPermissions =
            data.AudienceScopes
            |> Seq.collect (fun x -> x.Scopes)

        let accessToken =
            createAccessTokenClaims data.ClientId data.Issuer data.TokenData rolesPermissions audiences
            |> (createSignedToken data.AccessTokenSecret data.AccessTokenExpiresIn)
            
        let idToken =
            createIdTokenClaims data.ClientId data.Issuer data.TokenData rolesPermissions
            |> (createUnsignedToken data.IdTokenExpiresIn)

        let refreshToken = data.RefreshTokenProvider()

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

    type SignInScopes =
        | RequestedScopes of string seq
        | ValidatedScopes of AudienceScopes seq

    let private getValidatedScopes dataContext email clientId (scopes: SignInScopes) =
        task {
            match scopes with
            | RequestedScopes scopes -> return! validateScopes dataContext email clientId scopes
            | ValidatedScopes scopes -> return scopes
        }
        
    let getAudienceSecretAndExpire env aud =
        match aud = PERIMETER_USERS_AUDIENCE with
        | true -> Task.FromResult(int env.JwtConfig.AccessTokenExpiresIn, env.JwtConfig.AccessTokenSecret)
        | false ->
            query {
                for api in env.DataContext.Apis do
                    where (api.Identifier = aud)
                    select (api.AccessTokenExpiresIn, api.HS256SigningSecret)
            }
            |> toSingleAsync


    let signInUser env (tokenData: TokenData) clientId (scopes: SignInScopes) =
        task {

            let! { ClientId = clientId; Issuer = issuer; IdTokenExpiresIn = idTokenExpiresIn } =
                PRR.Domain.Auth.LogIn.UserHelpers.getAppInfo
                    env.DataContext
                    clientId
                    tokenData.Email
                    env.JwtConfig.IdTokenExpiresIn

            printfn "signInUser:1 %s %s %A" clientId issuer scopes

            let! validatedScopes = getValidatedScopes env.DataContext tokenData.Email clientId scopes

            printfn "signInUser:2 %A" validatedScopes

            // TODO !
            // We can create single access token for various apis only if they had exactly same config
            // Should move access token config data to the domain
            // And then override them if necessary for each config, this case when there
            // is requested scopes from different apis and some otf them overriden, we should
            // throw authorization request unsupported (think of message) exception

            let audiences =
                validatedScopes |> Seq.map (fun x -> x.Audience)

            if (Seq.length audiences = 0)
            then return raise (unAuthorized "Empty audience is not supported")

            let auds = audiences |> Seq.toList

            let aud =
                match auds with
                // first is perimeter audience and second is not, choose second
                | [ aud1; aud2 ] when aud1 = PERIMETER_USERS_AUDIENCE
                                      && aud2 <> PERIMETER_USERS_AUDIENCE -> aud2
                // only perimeter audience
                | [ aud1 ] when aud1 = PERIMETER_USERS_AUDIENCE -> aud1
                | _ -> raise (unAuthorized (sprintf "Unexpected audiences %A" auds))

            let! (accessTokenExpiresIn, hs256SigningSecret) = getAudienceSecretAndExpire env aud
            
            let data =
                { TokenData = tokenData
                  ClientId = clientId
                  Issuer = issuer
                  AudienceScopes = validatedScopes                  
                  // TODO !
                  RefreshTokenProvider = env.HashProvider
                  AccessTokenSecret = hs256SigningSecret
                  AccessTokenExpiresIn = accessTokenExpiresIn * 1<minutes>
                  IdTokenExpiresIn = idTokenExpiresIn }

            let result = signInUser' data

            return (result, clientId, aud)
        }
