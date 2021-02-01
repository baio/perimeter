namespace PRR.API.Tenant.Routes

open Giraffe

[<AutoOpen>]
module CreateRoutes =
    let createRoutes () =
        choose [ SocialConnections.createRoutes ()
                 Tenants.createRoutes ()
                 Permission.createRoutes ()
                 Role.createRoutes ()
                 Api.createRoutes ()
                 Application.createRoutes ()
                 Domain.createRoutes ()
                 DomainPool.createRoutes ()
                 TenantUserRole.createRoutes ()
                 DomainUserRole.createRoutes ()
                 UsersActivities.createRoutes ()
                 GetManagementDomainRoutes.createRoutes () ]
