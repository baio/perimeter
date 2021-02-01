namespace PRR.API.Tenant.Routes

open DataAvail.Http.Exceptions.Exceptions
open Microsoft.AspNetCore.Http
open PRR.Domain.Tenant.CreateUserTenant
open DataAvail.Giraffe.Common
open Microsoft.Extensions.Logging

module E2ERoutes =
    
    let createUserTenant (ctx: HttpContext) =               
        
        let env: Env = {
            DbDataContext = getDataContext ctx
            AuthConfig = (getConfig ctx).TenantAuth
            AuthStringsGetter = getAuthStringsGetter ctx
        }
        
        let userId = tryBindUserClaimId ctx
        let userEmail = tryBindUserEmail ctx
         
        match (userId, userEmail) with
        | Some(userId), Some(userEmail) ->
            createUserTenant env { UserId = userId; Email = userEmail }
        | _ as x ->
            (getLogger ctx).LogWarning("User Id or Email not found ${@data}", x)
            raise (Unexpected None)


    

