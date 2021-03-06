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
          SigningCredentials: SigningCredentials
          AccessTokenExpiresIn: int<minutes>
          IdTokenExpiresIn: int<minutes>
          Nonce: string }

    let signIn (data: SignInData) =

        let audiences =
            data.AudienceScopes
            |> Seq.map (fun x -> x.Audience)

        // TODO : What is audience ???
        // looks like audience is clientId
        let audiences =
            audiences |> Seq.append [| data.ClientId |]

        let rolesPermissions =
            data.AudienceScopes
            |> Seq.collect (fun x -> x.Scopes)
            |> Seq.distinct

        let accessToken =
            createAccessTokenClaims data.ClientId data.Issuer data.UserTokenData rolesPermissions audiences
            |> (createSignedToken data.SigningCredentials data.AccessTokenExpiresIn)

        let idToken =
            match data.UserTokenData with
            | Some userTokenData ->
                createIdTokenClaims data.ClientId data.Issuer data.Nonce userTokenData rolesPermissions audiences
                |> (createSignedToken data.SigningCredentials data.IdTokenExpiresIn)
            | None -> null

        let refreshToken =
            match data.RefreshTokenProvider with
            | Some refreshTokenProvider -> refreshTokenProvider ()
            | None -> null



        { id_token = idToken
          access_token = accessToken
          refresh_token = refreshToken }: LogInResult
