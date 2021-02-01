namespace PRR.API.Routes.Tenant

open Giraffe

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
