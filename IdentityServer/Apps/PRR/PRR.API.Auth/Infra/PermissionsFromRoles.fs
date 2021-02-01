namespace PRR.API.Auth.Infra

open PRR.Data.DataContext
open PRR.API.Auth
open DataAvail.EntityFramework.Common

[<AutoOpen>]
module PermissionsFromRoles =

    let getPermissions (dataContext: DbDataContext) (roles: int seq) =                
        query {
            for role in dataContext.Roles do
            where ((%(in') roles) role.Id) 
            join rolePermission in dataContext.RolesPermissions on (role.Id = rolePermission.RoleId)            
            select rolePermission.PermissionId
        } |> toSeqAsync

    type PermissionsFromRoles(dataContext: DbDataContext) = 
        interface IPermissionsFromRoles with
            member __.GetPermissions = getPermissions dataContext


