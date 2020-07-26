namespace PRR.API.Tests.Utils

open Npgsql.Logging

open System

[<AutoOpen>]
module EFLogger = 

    type ConsoleLogger()=
        inherit NpgsqlLogger()

        override __.IsEnabled(level: NpgsqlLogLevel) = true

        override __.Log(level: NpgsqlLogLevel, connectorId: int, msg: string, e: Exception) = printf "%s" msg

    type ConsoleLoggerProvider()=
        interface INpgsqlLoggingProvider with
            member __.CreateLogger(_: string) =
                ConsoleLogger() :> NpgsqlLogger

