namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open System.Security.Claims

[<AutoOpen>]
module private CreateClaims =

    let strJoin (s: string seq) = System.String.Join(" ", s).Trim()

    let createAccessTokenClaims clientId tokenData (scopes: string seq) (audiences: string seq) =

        let auds =
            audiences |> Seq.map (fun x -> Claim("aud", x))

        let permissions = scopes |> strJoin
        // TODO : RBA + Include permissions flag
        [| Claim("sub", tokenData.Id.ToString())
           Claim("scope", strJoin [ "openid"; permissions ])
           Claim(CLAIM_TYPE_CID, clientId) |]
        |> Seq.append auds

    let createIdTokenClaims clientId tokenData (scopes: string seq) =

        let permissions = scopes |> strJoin
        // TODO : RBA + Include permissions flag
        [| Claim("sub", tokenData.Id.ToString())
           Claim(ClaimTypes.Email, tokenData.Email)
           // TODO : Separate claims for access and id
           Claim(ClaimTypes.GivenName, tokenData.FirstName)
           Claim(ClaimTypes.Surname, tokenData.LastName)
           Claim(CLAIM_TYPE_CID, clientId)
           Claim("scope", strJoin [ "openid"; permissions ]) |]
