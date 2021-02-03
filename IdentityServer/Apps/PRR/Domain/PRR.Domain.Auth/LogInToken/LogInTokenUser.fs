namespace PRR.Domain.Auth.LogInToken

open System.Security.Cryptography
open System.Threading.Tasks
open PRR.Domain.Models

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open Models
open PRR.Data.DataContext
open PRR.Data.Entities
open PRR.Domain.Auth
open PRR.Domain.Auth.LogIn.UserHelpers
open PRR.Domain.Auth.LogInToken
open DataAvail.EntityFramework.Common
open PRR.Domain.Auth.Common
open DataAvail.Http.Exceptions

type SignInUserEnv =
    { DataContext: DbDataContext
      JwtConfig: JwtConfig
      Logger: ILogger
      HashProvider: HashProvider }

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
          AccessTokenCredentials: SigningCredentials
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
            |> (createSignedToken data.AccessTokenCredentials data.AccessTokenExpiresIn)

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
            | Some { DomainId = domainId
                     Audiences = audiences } ->
                let! rolesPermissions = getUserDomainRolesPermissions dataContext (domainId, email)
                return (audiences, (rolesPermissions :> seq<_>))
            | None -> return raise (unAuthorized "Client is not found")
        }


    let private perimeterUserRolePermissions =
        [| { Role = "PerimeterUser"
             Permissions = [ "profile"; "email" ] } |]
        |> Array.toSeq

    let private perimeterAudiences =
        [| PERIMETER_USERS_AUDIENCE |] |> Array.toSeq

    let private getClientAudiencesRolePermissions (dataContext: DbDataContext) clientId email =

        task {
            if clientId = PERIMETER_CLIENT_ID then
                return (perimeterAudiences, perimeterUserRolePermissions)
            else
                let! (audiences, rolesPermissions) = getClientAudiencesRolePermissions' dataContext clientId email

                return
                    (Seq.append audiences perimeterAudiences),
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


    let private createHS256Credentials (secret: string) =
        SigningCredentials((createHS256Key secret), SecurityAlgorithms.HmacSha256Signature)

    let private createRS256Credentials (xmlParams: string) =
        SigningCredentials((createRS256Key xmlParams), SecurityAlgorithms.RsaSha256)

    type DomainSecretData =
        { AccessTokenExpiresIn: int
          SigningCredentials: SigningCredentials }

    let getDomainSecretAndExpire env issuer isPerimeterClient =
        task {
            // perimeter clients should always use internal credentials
            match isPerimeterClient with
            | true ->
                return
                    { AccessTokenExpiresIn = int env.JwtConfig.AccessTokenExpiresIn
                      SigningCredentials = createHS256Credentials env.JwtConfig.AccessTokenSecret }
            | false ->
                let! (expiresIn, algo, hs256key, rs256params) =
                    query {
                        for domain in env.DataContext.Domains do
                            where (domain.Issuer = issuer)

                            select
                                (domain.AccessTokenExpiresIn,
                                 domain.SigningAlgorithm,
                                 domain.HS256SigningSecret,
                                 domain.RS256Params)
                    }
                    |> toSingleAsync

                return
                    { AccessTokenExpiresIn = expiresIn
                      SigningCredentials =
                          if algo = SigningAlgorithmType.HS256 then
                              createHS256Credentials hs256key
                          else
                              createRS256Credentials rs256params }
        }


    let signInUser env (tokenData: TokenData) clientId (scopes: SignInScopes) =
        task {

            let! { ClientId = clientId
                   Issuer = issuer
                   IdTokenExpiresIn = idTokenExpiresIn
                   Type = clientType } =
                getAppInfo env.DataContext clientId tokenData.Email env.JwtConfig.IdTokenExpiresIn

            let! validatedScopes = getValidatedScopes env.DataContext tokenData.Email clientId scopes

            let audiences =
                validatedScopes |> Seq.map (fun x -> x.Audience)

            if (Seq.length audiences = 0)
            then return raise (unAuthorized "Empty audience is not supported")

            let isPerimeterClient = clientType <> Regular

            let! secretData = getDomainSecretAndExpire env issuer isPerimeterClient

            let data =
                { TokenData = tokenData
                  ClientId = clientId
                  Issuer = issuer
                  AudienceScopes = validatedScopes
                  RefreshTokenProvider = env.HashProvider
                  AccessTokenCredentials = secretData.SigningCredentials
                  AccessTokenExpiresIn = secretData.AccessTokenExpiresIn * 1<minutes>
                  IdTokenExpiresIn = idTokenExpiresIn }

            let result = signInUser' data

            return (result, clientId, isPerimeterClient)
        }
