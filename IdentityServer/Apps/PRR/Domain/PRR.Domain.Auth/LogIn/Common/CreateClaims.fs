namespace PRR.Domain.Auth.LogIn.Common

open PRR.Domain.Models
open System.Security.Claims
open PRR.Domain.Auth.LogIn.Common

[<AutoOpen>]
module private CreateClaims =

    let strJoin (s: string seq) = System.String.Join(" ", s).Trim()

    let getSocialSubPrefix =
        function
        | Github -> "github"
        | Twitter -> "twitter"
        | Google -> "google"

    let getSubPrefix =
        function
        | Some socialType -> getSocialSubPrefix socialType
        | None -> "prr"

    let getSub tokenData =
        sprintf "%s|%s" (getSubPrefix tokenData.SocialType) tokenData.Email

    let createAccessTokenClaims clientId issuer tokenData (scopes: string seq) (audiences: string seq) =

        let audiencesClaims =
            audiences
            |> Seq.map (fun x -> Claim(CLAIM_TYPE_AUDIENCE, x))

        let permissions = scopes |> strJoin

        let userClaims =
            match tokenData with
            | Some tokenData ->
                [| Claim(CLAIM_TYPE_SUB, getSub tokenData)
                   Claim(ClaimTypes.Email, tokenData.Email)
                   Claim(CLAIM_TYPE_UID, tokenData.Id.ToString()) |]
            | None -> [||]

        // TODO : RBA + Include permissions flag
        [| Claim(CLAIM_TYPE_SCOPE, permissions)
           Claim(CLAIM_TYPE_ISSUER, issuer)
           Claim(CLAIM_TYPE_CID, clientId) |]
        |> Seq.append userClaims
        |> Seq.append audiencesClaims

    let createIdTokenClaims clientId issuer tokenData (scopes: string seq) =

        let permissions = scopes |> strJoin
        // TODO : RBA + Include permissions flag
        [| Claim(CLAIM_TYPE_SUB, getSub tokenData)
           Claim(ClaimTypes.Email, tokenData.Email)
           // TODO : Separate claims for access and id
           Claim(ClaimTypes.GivenName, tokenData.FirstName)
           Claim(ClaimTypes.Surname, tokenData.LastName)
           Claim(CLAIM_TYPE_CID, clientId)
           Claim(CLAIM_TYPE_SCOPE, permissions)
           Claim(CLAIM_TYPE_ISSUER, issuer) |]
