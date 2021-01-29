namespace PRR.Domain.Common

open System
open PRR.Domain.Models

module Events =

    [<CLIMutable>]
    type LogIn =
        { Social: Social option
          DateTime: DateTime
          UserId: int
          ClientId: string
          DomainId: int
          IsManagementClient: bool
          AppIdentifier: string
          UserEmail: string }
