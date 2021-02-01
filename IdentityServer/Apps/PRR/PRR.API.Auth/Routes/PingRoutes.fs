namespace PRR.API.Auth.Routes

open Giraffe

module PingRoutes =

    let createRoutes () =
        route "/auth/version"
        >=> GET
        >=> (fun next ctx ->
            json
                {| Version = 20
                   Environment = ctx.GetHostingEnvironment().EnvironmentName |}
                next
                ctx)
