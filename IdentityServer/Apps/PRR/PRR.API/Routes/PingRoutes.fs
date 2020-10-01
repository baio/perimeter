﻿namespace PRR.API.Routes

open Giraffe

module PingRoutes =

    let createRoutes () =
        route "/version"
        >=> GET
        >=> (fun next ctx ->
            json
                {| Version = 19
                   Environment = ctx.GetHostingEnvironment().EnvironmentName |}
                next
                ctx)
