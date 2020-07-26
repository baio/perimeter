namespace PRR.API.Routes.Tenant

open Giraffe

module Tenant =
    let createRoutes() =
        choose
            [
              Permission.createRoutes()
              Role.createRoutes()
              Api.createRoutes()
              Application.createRoutes()
              Domain.createRoutes()
              DomainPool.createRoutes()
              TenantUserRole.createRoutes()
              DomainUserRole.createRoutes() ]
