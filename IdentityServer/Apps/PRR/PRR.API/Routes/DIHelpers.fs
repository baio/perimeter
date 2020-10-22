namespace PRR.API.Routes

open PRR.Data.DataContext
open Giraffe
open Microsoft.AspNetCore.Http
open Common.Utils
open Common.Utils.ReaderTask
open PRR.API.Infra.Models
open PRR.Sys.SetUp

[<AutoOpen>]
module DIHelpers =
    let getDataContext (ctx: HttpContext) = ctx.GetService<DbDataContext>()

    let getDataContext': ReaderTask<HttpContext, DbDataContext> = ofReader getDataContext

    let getHash (ctx: HttpContext) = ctx.GetService<IHashProvider>().GetHash

    let getSHA256 (ctx: HttpContext) =
        ctx.GetService<ISHA256Provider>().GetSHA256

    let getPasswordSalter (ctx: HttpContext) =
        ctx.GetService<IPasswordSaltProvider>().SaltPassword

    let getConfig (ctx: HttpContext) = ctx.GetService<IConfig>().GetConfig()


    let getSystemActors (ctx: HttpContext) =
        ctx.GetService<ISystemActorsProvider>().SystemActors

    let getAuthStringsProvider (ctx: HttpContext) =
        ctx.GetService<IAuthStringsProvider>().AuthStringsProvider

    let getViewsReaderDb (ctx: HttpContext) =
        ctx.GetService<IViewsReaderDbProvider>().ViewsReaderDb

    let getHttpRequestFun (ctx: HttpContext) =
        ctx.GetService<IHttpRequestFunProvider>().HttpRequestFun
