namespace PRR.API.Common.Configuration

open System
open MassTransit
open MassTransit.RabbitMqTransport
open Microsoft.Extensions.DependencyInjection
open MongoDB.Driver

[<AutoOpen>]
module ServiceBus =

    type Config = { Host: string }

    let configureServiceBus (config: Config) (consumers: Type seq) (services: IServiceCollection) =
        services.AddMassTransit(fun x ->
            consumers |> Seq.iter (x.AddConsumer)

            x.UsingRabbitMq(fun ctx cfg ->
                cfg.Host(config.Host)

                if (Seq.length consumers) > 0 then
                    cfg.ReceiveEndpoint
                        ("event-listener", (fun (e: IRabbitMqReceiveEndpointConfigurator) -> e.ConfigureConsumers(ctx)))))
        |> ignore

        services.AddMassTransitHostedService() |> ignore
