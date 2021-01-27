namespace PRR.API.Configuration

open MassTransit
open Microsoft.Extensions.DependencyInjection

[<AutoOpen>]
module private ServiceBus =

    let configureServiceBus () (services: IServiceCollection) =
        services.AddMassTransit(fun x -> x.UsingRabbitMq())
        services.AddMassTransitHostedService()
