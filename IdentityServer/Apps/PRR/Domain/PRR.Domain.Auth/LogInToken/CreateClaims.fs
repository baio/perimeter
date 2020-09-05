namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open System.Security.Claims

[<AutoOpen>]
module private CreateClaims =

    let strJoin (s: string seq) = System.String.Join(" ", s).Trim()

    let createAccessTokenClaims clientId issuer tokenData (scopes: string seq) (audiences: string seq) =

        let auds =
            audiences
            |> Seq.map (fun x -> Claim(CLAIM_TYPE_AUDIENCE, x))

        let permissions = scopes |> strJoin
        // TODO : RBA + Include permissions flag
        [| Claim(CLAIM_TYPE_SUB, (sprintf "prr|%s" tokenData.Email))
           Claim(CLAIM_TYPE_UID, tokenData.Id.ToString())
           Claim(CLAIM_TYPE_SCOPE, strJoin [ "openid"; permissions ])
           Claim(CLAIM_TYPE_ISSUER, issuer)
           Claim(CLAIM_TYPE_CID, clientId) |]
        |> Seq.append auds

    let createIdTokenClaims clientId  issuer tokenData (scopes: string seq) =

        let permissions = scopes |> strJoin
        // TODO : RBA + Include permissions flag
        [|
           Claim(CLAIM_TYPE_SUB, (sprintf "prr|%s" tokenData.Email))
           Claim(ClaimTypes.Email, tokenData.Email)
           // TODO : Separate claims for access and id
           Claim(ClaimTypes.GivenName, tokenData.FirstName)
           Claim(ClaimTypes.Surname, tokenData.LastName)
           Claim(CLAIM_TYPE_CID, clientId)
           Claim(CLAIM_TYPE_SCOPE, strJoin [ "openid"; permissions ])
           Claim(CLAIM_TYPE_ISSUER, issuer) |]
