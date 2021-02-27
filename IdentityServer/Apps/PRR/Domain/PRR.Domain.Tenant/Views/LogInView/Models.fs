namespace PRR.Domain.Tenant.Views.LogInView

open System
open MongoDB.Bson
open PRR.Domain.Models

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type LogInDoc =
        { // TODO : Id: string
          // Denormalized version of Login event         
          Social: Social option
          DateTime: DateTime
          UserId: int
          UserEmail: string
          ClientId: string
          DomainId: int
          IsManagementClient: bool
          AppIdentifier: string
          GrantType: string }
