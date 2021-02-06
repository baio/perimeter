namespace PRR.API.Tenant.EventHandlers

open MassTransit
open MongoDB.Driver
open PRR.API.Common.Infra.ViewsDbProvider
open PRR.Domain.Common
open PRR.Domain.Tenant.Views.LogInView
open Microsoft.Extensions.Logging

[<AutoOpen>]
module LogInEventHandler =

    type LogInEventHandler(dbProvider: IViewsDbProvider) =
        interface IConsumer<Events.LogIn> with
            member __.Consume context =
                printfn("LogInEventHandler")
                insertLoginEvent dbProvider.Db context.Message
