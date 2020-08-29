namespace Common.Domain.Giraffe

open Common.Domain.Models
open Common.Utils
open FSharpx.Option
open Microsoft.AspNetCore.Http
open System.Security.Claims

[<AutoOpen>]
module User =

    let bindUser (ctx: HttpContext) = ctx.Request.HttpContext.User

    let bindUserClaims x =
        x
        |> bindUser
        |> (fun user -> user.Claims)

    let claimValue (claim: Claim) = claim.Value
    let claimType (claim: Claim) = claim.Type

    let tryBindUserClaim' fn =
        bindUserClaims
        >> Seq.tryFind (fun x -> x.Type |> fn)
        >> Option.map claimValue

    let tryBindUserClaim name = tryBindUserClaim' (fun x -> x = name)

    let tryBindUserClaimInt name = tryBindUserClaim name >=> tryParseInt


    let tryBindUserClaimId x = x |> tryBindUserClaimInt ClaimTypes.NameIdentifier

    let tryBindUserEmail x = x |> tryBindUserClaim ClaimTypes.Email

    let bindUserClaimId x =
        x
        |> tryBindUserClaimId
        |> option2Task (forbidden "User sub is not found")

    let tryBindUserClaimDomainId x = x |> tryBindUserClaimInt CLAIM_TYPE_DOMAIN

    let tryBindUserClaimClientId x = x |> tryBindUserClaim CLAIM_TYPE_CID

    let tryBindUserClaimDomainId' x =
        x
        |> tryBindUserClaimDomainId
        |> option2Task Forbidden'

    let tryBindUserClaimId' x =
        x
        |> tryBindUserClaimId
        |> option2Task Forbidden'

    let bindUserClaimsFilter<'a> fn =
        bindUserClaims
        >> Seq.filter (claimType >> fn)
        >> Seq.map (claimValue)

    let bindUserClaimsRoles x =
        x
        |> bindUserClaimsFilter (fun x -> x = ClaimTypes.Role)
        |> Seq.choose (tryParseInt)

    let bindUserClaimsAudiences x =
        x |> bindUserClaimsFilter (fun x -> x = "aud")

    let bindUserClaimsScopes x =
        x
        |> bindUserClaimsFilter (fun x -> x = "scope")
        |> Seq.tryHead
        |> Option.map (fun s -> s.Split(" "))
        |> function
        | Some x -> x
        | None -> [||]
