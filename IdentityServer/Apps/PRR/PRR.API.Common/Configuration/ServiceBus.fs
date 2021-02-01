namespace PRR.API.Common.Configuration

open System
open MassTransit
open MassTransit.RabbitMqTransport
open Microsoft.Extensions.DependencyInjection
open MongoDB.Driver

[<AutoOpen>]
module ServiceBus =

    let configureServiceBus (consumers: Type seq) (services: IServiceCollection) =
        services.AddMassTransit(fun x ->
            consumers |> Seq.iter (x.AddConsumer)

            x.UsingRabbitMq(fun ctx cfg ->
                cfg.ReceiveEndpoint
                    ("event-listener", (fun (e: IRabbitMqReceiveEndpointConfigurator) -> e.ConfigureConsumers(ctx)))))
        |> ignore

        services.AddMassTransitHostedService() |> ignore
