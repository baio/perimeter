namespace PRR.Domain.Auth.SignIn

open Common.Domain.Models
open System.Security.Claims

[<AutoOpen>]
module private CreateClaims =


    let createAccessTokenClaims tokenData (rolePermissions: RolePermissions seq) (audiences: string seq) =
        let roles =
            rolePermissions
            |> Seq.map (fun x -> x.Role)
            |> Seq.map (fun x -> Claim(ClaimTypes.Role, x))

        let auds =
            audiences |> Seq.map (fun x -> Claim("aud", x))

        let permissions =
            rolePermissions
            |> Seq.collect (fun x -> x.Permissions)
            |> String.concat " "

        // TODO : RBA + Include permissions flag
        [| Claim("sub", tokenData.Id.ToString())
           Claim("scope", sprintf "openid roles %s" permissions) |]
        |> Seq.append roles
        |> Seq.append auds

    let createIdTokenClaims tokenData (rolePermissions: RolePermissions seq) =
        let roles =
            rolePermissions
            |> Seq.map (fun x -> x.Role)
            |> Seq.map (fun x -> Claim(ClaimTypes.Role, x))

        let permissions =
            rolePermissions
            |> Seq.collect (fun x -> x.Permissions)
            |> String.concat " "
        // TODO : RBA + Include permissions flag
        [| Claim("sub", tokenData.Id.ToString())
           Claim(ClaimTypes.Email, tokenData.Email)
           // TODO : Separate claims for access and id
           Claim(ClaimTypes.Name, sprintf "%s %s" tokenData.FirstName tokenData.LastName)
           Claim("scope", sprintf "openid profile roles %s" permissions) |]
        |> Seq.append roles
