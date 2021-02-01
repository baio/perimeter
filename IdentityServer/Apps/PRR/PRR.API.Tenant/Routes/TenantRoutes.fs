namespace PRR.API.Tenant.Routes

open Giraffe
open DataAvail.Giraffe.Common

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
                 GetManagementDomainRoutes.createRoutes ()
                 GET >=> route "/version" >=> Version.handler
#if E2E
                 POST
                 >=> route "/e2e/create-user-tenant"
                 >=> wrapHandlerOK E2ERoutes.createUserTenant
#endif
                  ]
