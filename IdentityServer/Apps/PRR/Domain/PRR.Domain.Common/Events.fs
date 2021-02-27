namespace PRR.Domain.Common

open System
open PRR.Domain.Models

module Events =

    // TODO : Add audience and scope

    type LoginUser = { UserId: int; UserEmail: string }

    type LogInGrantType =
        | AuthorizationCode of LoginUser
        | AuthorizationCodePKCE of LoginUser
        | Password of LoginUser
        | Social of LoginUser * Social
        | ClientCredentials

    [<CLIMutable>]
    type LogInEvent =
        { DateTime: DateTime
          ClientId: string
          DomainId: int
          IsManagementClient: bool
          AppIdentifier: string
          GrantType: LogInGrantType }
