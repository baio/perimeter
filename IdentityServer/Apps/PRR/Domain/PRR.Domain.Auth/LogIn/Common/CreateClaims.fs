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

        let hasEmailScope = scopes |> Seq.contains "email"

        let userClaims =
            match tokenData with
            | Some tokenData ->
                [| Some(Claim(CLAIM_TYPE_SUB, getSub tokenData))
                   Some(Claim(CLAIM_TYPE_UID, tokenData.Id.ToString()))
                   if hasEmailScope
                   then Some(Claim(ClaimTypes.Email, tokenData.Email))
                   else None |]
                |> Array.choose id
            | None -> [||]


        // TODO : RBA + Include permissions flag
        [| Claim(CLAIM_TYPE_SCOPE, permissions)
           Claim(CLAIM_TYPE_ISSUER, issuer)
           Claim(CLAIM_TYPE_CID, clientId)
           Claim(CLAIM_TYPE_NONCE, clientId) |]
        |> Seq.append userClaims
        |> Seq.append audiencesClaims

    let createIdTokenClaims clientId issuer nonce tokenData (scopes: string seq) (audiences: string seq) =

        let hasEmailScope = scopes |> Seq.contains "email"
        let hasProfileScope = scopes |> Seq.contains "profile"

        let permissions = scopes |> strJoin

        let audiencesClaims =
            audiences
            |> Seq.map (fun x -> Claim(CLAIM_TYPE_AUDIENCE, x))

        let userClaims =
            [| if hasEmailScope
               then Some(Claim(ClaimTypes.Email, tokenData.Email))
               else None
               // TODO : Separate claims for access and id
               if hasProfileScope
               then Some(Claim(ClaimTypes.GivenName, tokenData.FirstName))
               else None
               if hasProfileScope
               then Some(Claim(ClaimTypes.Surname, tokenData.LastName))
               else None |]
            |> Array.choose id

        let nonceClaims =
            match nonce with
            | null -> [||]
            | nonce -> [| Claim(CLAIM_TYPE_NONCE, nonce) |]            

        // TODO : RBA + Include permissions flag
        [| Claim(CLAIM_TYPE_SUB, getSub tokenData)
           Claim(CLAIM_TYPE_CID, clientId)
           Claim(CLAIM_TYPE_SCOPE, permissions)
           Claim(CLAIM_TYPE_ISSUER, issuer) |]
        |> Array.append userClaims
        |> Seq.append audiencesClaims
        |> Seq.append nonceClaims
