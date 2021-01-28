namespace PRR.API.EventHandlers

open MassTransit
open MongoDB.Driver
open PRR.API.Infra.Models
open PRR.Domain.Common
open PRR.Domain.Tenant.Views.LogInView

[<AutoOpen>]
module LogInEventHandler =

    type LogInEventHandler(dbProvider: IViewsDbProvider) =
        interface IConsumer<Events.LogIn> with
            member __.Consume context =
                insertLoginEvent dbProvider.Db context.Message
