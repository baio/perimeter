namespace PRR.Domain.Auth.LogIn.Common

open Microsoft.IdentityModel.Tokens
open PRR.Domain.Models

[<AutoOpen>]
module SignIn =

    type SignInData =
        { UserTokenData: UserTokenData option
          RefreshTokenProvider: (unit -> string) option
          ClientId: ClientId
          Issuer: string
          AudienceScopes: AudienceScopes seq
          AccessTokenCredentials: SigningCredentials
          AccessTokenExpiresIn: int<minutes>
          IdTokenExpiresIn: int<minutes> }
        
    let signIn (data: SignInData) =

        let audiences =
            data.AudienceScopes
            |> Seq.map (fun x -> x.Audience)

        let rolesPermissions =
            data.AudienceScopes
            |> Seq.collect (fun x -> x.Scopes)

        let accessToken =
            createAccessTokenClaims data.ClientId data.Issuer data.UserTokenData rolesPermissions audiences
            |> (createSignedToken data.AccessTokenCredentials data.AccessTokenExpiresIn)

        let idToken =
            match data.UserTokenData with
            | Some userTokenData ->
                createIdTokenClaims data.ClientId data.Issuer userTokenData rolesPermissions
                |> (createUnsignedToken data.IdTokenExpiresIn)
            | None -> null

        let refreshToken =
            match data.RefreshTokenProvider with
            | Some refreshTokenProvider -> refreshTokenProvider ()
            | None -> null


        { id_token = idToken
          access_token = accessToken
          refresh_token = refreshToken }: LogInResult
