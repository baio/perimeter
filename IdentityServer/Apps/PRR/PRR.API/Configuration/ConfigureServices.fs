﻿namespace PRR.API.Configuration

open Microsoft.Extensions.DependencyInjection

[<AutoOpen>]
module ConfigureServices =

    let configureServices' (services: IServiceCollection) =
            configureLogging services
            configureTracing services

