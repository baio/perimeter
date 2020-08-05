namespace Common.Domain.Giraffe

open Common.Domain.Models
open Giraffe

[<AutoOpen>]
module Auth =

    let auth'' f permission: HttpHandler =
        fun next ctx ->
            let scopes = ctx |> bindUserClaimsScopes
            let hasPermission = scopes |> Seq.contains permission 
            match hasPermission with
            | true ->
                next ctx
            | false ->
                f next ctx

    let auth' = auth'' (fun _ _ -> raise Forbidden)

    let authOpt x = auth'' (fun _ _ ->
        System.Threading.Tasks.Task.FromResult(None)) x

    let notLoggedIn: HttpHandler = fun _ _ -> raise (UnAuthorized None)

    let requiresAuth = requiresAuthentication notLoggedIn

    let permissionGuard permission = requiresAuth >=> auth' permission

    let permissionOptGuard permission = requiresAuth >=> authOpt permission
