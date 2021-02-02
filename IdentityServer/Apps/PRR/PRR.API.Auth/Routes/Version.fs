namespace PRR.API.Auth.Routes

open Giraffe
open Microsoft.AspNetCore.Http

module Version =
    let handler =
        fun next (ctx: HttpContext) ->
            json
                {| Version = 20
                   Environment = ctx.GetHostingEnvironment().EnvironmentName |}
                next
                ctx
