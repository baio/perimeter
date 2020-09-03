namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open System.Security.Claims

[<AutoOpen>]
module private CreateClaims =
    
    let strJoin (s: string seq) = System.String.Join(" ", s) 

    let createAccessTokenClaims clientId tokenData (rolePermissions: RolePermissions seq) (audiences: string seq) =
        let roles =
            rolePermissions
            |> Seq.map (fun x -> x.Role)
            |> Seq.map (fun x -> Claim(ClaimTypes.Role, x))

        let auds =
            audiences |> Seq.map (fun x -> Claim("aud", x))

        let permissions =
            rolePermissions
            |> Seq.collect (fun x -> x.Permissions)
            |> strJoin

        // TODO : RBA + Include permissions flag
        [| Claim("sub", tokenData.Id.ToString())
           Claim("scope", sprintf "openid roles %s" permissions) 
           Claim(CLAIM_TYPE_CID, clientId) |]
        |> Seq.append roles
        |> Seq.append auds

    let createIdTokenClaims clientId tokenData (rolePermissions: RolePermissions seq) =
        let roles =
            rolePermissions
            |> Seq.map (fun x -> x.Role)
            |> Seq.map (fun x -> Claim(ClaimTypes.Role, x))

        let permissions =
            rolePermissions
            |> Seq.collect (fun x -> x.Permissions)
            |> strJoin
        // TODO : RBA + Include permissions flag
        [| Claim("sub", tokenData.Id.ToString())
           Claim(ClaimTypes.Email, tokenData.Email)
           // TODO : Separate claims for access and id
           Claim(ClaimTypes.GivenName, tokenData.FirstName)
           Claim(ClaimTypes.Surname, tokenData.LastName)
           Claim(CLAIM_TYPE_CID, clientId)
           Claim("scope", sprintf "openid roles %s" permissions) |]
        |> Seq.append roles
