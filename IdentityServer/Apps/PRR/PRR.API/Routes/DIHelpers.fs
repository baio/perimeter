﻿namespace PRR.API.Routes
open PRR.Data.DataContext
open Giraffe
open Microsoft.AspNetCore.Http
open Common.Utils
open Common.Utils.ReaderTask
open PRR.API.Infra.Models

[<AutoOpen>]
module DIHelpers =    
    let getDataContext (ctx: HttpContext) = ctx.GetService<DbDataContext>()

    let getDataContext': ReaderTask<HttpContext, DbDataContext> = ReaderTask.ofReader getDataContext

    let getHash (ctx: HttpContext) = ctx.GetService<IHashProvider>().GetHash
    let getPasswordSalter (ctx: HttpContext) = ctx.GetService<IPasswordSaltProvider>().SaltPassword
    let getConfig (ctx: HttpContext) = ctx.GetService<IConfig>().GetConfig()


