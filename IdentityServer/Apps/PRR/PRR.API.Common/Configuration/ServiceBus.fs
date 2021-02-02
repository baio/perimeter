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
                if (Seq.length consumers) > 0 then
                    cfg.ReceiveEndpoint
                        ("event-listener",
                         (fun (e: IRabbitMqReceiveEndpointConfigurator) ->
                             consumers
                             |> Seq.iter (fun t -> e.ConfigureConsumer(ctx, t))))))
        |> ignore

        services.AddMassTransitHostedService() |> ignore
