namespace PRR.API.Common.Configuration

open Microsoft.Extensions.DependencyInjection
open PRR.API.Common.Infra

[<AutoOpen>]
module ConfigProvider =

    let configureConfigProvider (config: 'a) (services: IServiceCollection) =
        services.AddSingleton<IConfigProvider<'a>>(ConfigProvider config)
        |> ignore
