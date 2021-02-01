namespace PRR.API.Tenant.EventHandlers

open MassTransit
open MongoDB.Driver
open PRR.API.Common.Infra.ViewsDbProvider
open PRR.Domain.Common
open PRR.Domain.Tenant.Views.LogInView

[<AutoOpen>]
module LogInEventHandler =

    type LogInEventHandler(dbProvider: IViewsDbProvider) =
        interface IConsumer<Events.LogIn> with
            member __.Consume context =
                insertLoginEvent dbProvider.Db context.Message
