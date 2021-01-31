﻿namespace PRR.API.Tenant.Configuration

open Microsoft.Extensions.Configuration
open PRR.API.Common.Configuration
open PRR.Domain.Models
open PRR.Domain.Tenant


[<AutoOpen>]
module CreateAppConfig =

    let createAppConfig (configuration: IConfiguration) =

        let common = createCommonAppConfig configuration

        let accessTokenSecret =
            configuration.GetValue<string>("Auth:AccessTokenSecret")

        let auth: AuthConfig =
            { AccessTokenSecret = configuration.GetValue<string>("TenantAuth:AccessTokenSecret")
              IdTokenExpiresIn = configuration.GetValue<int<minutes>>("TenantAuth:IdTokenExpiresInMinutes")
              AccessTokenExpiresIn = configuration.GetValue<int<minutes>>("TenantAuth:IdTokenExpiresInMinutes")
              RefreshTokenExpiresIn = configuration.GetValue<int<minutes>>("TenantAuth:RefreshTokenExpiresInMinutes") }

        { Common = common
          AccessTokenSecret = accessTokenSecret
          TenantAuth = auth }
