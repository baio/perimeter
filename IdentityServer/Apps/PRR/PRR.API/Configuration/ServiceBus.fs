namespace PRR.API.Configuration

open System
open MassTransit
open MassTransit.RabbitMqTransport
open Microsoft.Extensions.DependencyInjection
open MongoDB.Driver
open PRR.API.EventHandlers

[<AutoOpen>]
module private ServiceBus =

    let configureServiceBus (services: IServiceCollection) =
        services.AddMassTransit(fun x ->
            x.AddConsumer<LogInEventHandler>() |> ignore
            x.UsingRabbitMq(fun ctx cfg ->
                cfg.ReceiveEndpoint
                    ("event-listener",
                     (fun (e: IRabbitMqReceiveEndpointConfigurator) -> e.ConfigureConsumer<LogInEventHandler>(ctx)))))
        services.AddMassTransitHostedService()
