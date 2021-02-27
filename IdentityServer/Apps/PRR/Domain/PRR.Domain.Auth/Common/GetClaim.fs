namespace PRR.Domain.Auth.Common


[<AutoOpen>]
module GetClaim =

    open System.Security.Claims
    open DataAvail.Common.StringUtils
    open DataAvail.Common.Option

    let getClaim' f claimType (claims: Claim seq) =
        claims
        |> Seq.tryFind (fun x -> x.Type = claimType)
        >>= fun claim -> claim.Value |> f

    let getClaim x = x |> getClaim' Some

    let getClaimInt x = x |> getClaim' tryParseInt
