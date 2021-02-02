namespace PRR.API.Auth.Routes

open DataAvail.KeyValueStorage.Core
open MassTransit
open PRR.API.Auth.Configuration.ConfigureServices
open PRR.API.Auth.Infra.Mail.Models
open PRR.Data.DataContext
open Giraffe
open Microsoft.AspNetCore.Http
open DataAvail.Common.ReaderTask
open PRR.API.Auth.Infra.Models

[<AutoOpen>]
module DIHelpers =
    let getDataContext (ctx: HttpContext) = ctx.GetService<DbDataContext>()

    let getDataContext': ReaderTask<HttpContext, DbDataContext> = ofReader getDataContext

    let getHash (ctx: HttpContext) = ctx.GetService<IHashProvider>().GetHash

    let getSHA256 (ctx: HttpContext) =
        ctx.GetService<ISHA256Provider>().GetSHA256

    let getPasswordSalter (ctx: HttpContext) =
        ctx.GetService<IPasswordSaltProvider>().SaltPassword

    let getConfig (ctx: HttpContext) =
        ctx.GetService<IConfigProvider>().GetConfig()

    let getHttpRequestFun (ctx: HttpContext) =
        ctx.GetService<IHttpRequestFunProvider>().HttpRequestFun

    let getLogger (ctx: HttpContext) = ctx.GetLogger()

    let getKeyValueStorage (ctx: HttpContext) = ctx.GetService<IKeyValueStorage>()

    let getSendMail (ctx: HttpContext) =
        ctx.GetService<ISendMailProvider>().GetSendMail()

    let getPublishEndpoint (ctx: HttpContext) = ctx.GetService<IPublishEndpoint>()
