﻿namespace PRR.API.Auth.Configuration

open Microsoft.Extensions.DependencyInjection
open PRR.API.Auth
open PRR.API.Auth.Infra
open System.Security.Cryptography
open DataAvail.HttpRequest.HttpFs

type InfraConfig =
    { PasswordSecret: string }

[<AutoOpen>]
module private Infra =

    let configureInfra (config: InfraConfig) (services: IServiceCollection) =

        let sha256 = SHA256.Create()
        let hashProvider = HashProvider sha256
        let sha256Provider = SHA256Provider sha256

        services.AddSingleton<IPermissionsFromRoles, PermissionsFromRoles>()
        |> ignore

        services.AddSingleton<IHashProvider>(hashProvider)
        |> ignore

        services.AddSingleton<ISHA256Provider>(sha256Provider)
        |> ignore

        services.AddSingleton<IPasswordSaltProvider>(PasswordSaltProvider config.PasswordSecret)
        |> ignore

        services.AddSingleton<IHttpRequestFunProvider>(HttpRequestFunProvider(httpFsRequestFun))
        |> ignore
