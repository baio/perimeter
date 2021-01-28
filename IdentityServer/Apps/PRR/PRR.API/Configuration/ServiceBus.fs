namespace PRR.API.Configuration

open MassTransit
open Microsoft.Extensions.DependencyInjection
open MongoDB.Driver
open PRR.API.EventHandlers

[<AutoOpen>]
module private ServiceBus =

    let configureServiceBus (services: IServiceCollection) =       
        services.AddMassTransit(fun x ->
            x.AddConsumer<LogInEventHandler>() |> ignore
            x.UsingRabbitMq())
        |> ignore
        services.AddMassTransitHostedService()
