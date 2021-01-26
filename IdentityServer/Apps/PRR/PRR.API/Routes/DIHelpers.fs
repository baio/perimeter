namespace PRR.API.Routes

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Configuration
open PRR.API.Configuration.ConfigureServices
open PRR.API.Infra.Mail.Models
open PRR.Data.DataContext
open Giraffe
open Microsoft.AspNetCore.Http
open Common.Utils
open Common.Utils.ReaderTask
open PRR.API.Infra.Models
open PRR.System.Models.CQRSSystem

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

    let getAuthStringsGetter (ctx: HttpContext) =
        ctx.GetService<IAuthStringsProvider>().AuthStringsGetter

    let getViewsReaderDb (ctx: HttpContext) =
        ctx.GetService<IViewsReaderDbProvider>().ViewsReaderDb

    let getHttpRequestFun (ctx: HttpContext) =
        ctx.GetService<IHttpRequestFunProvider>().HttpRequestFun

    let getLogger (ctx: HttpContext) = ctx.GetLogger()
    
    let getKeyValueStorage (ctx: HttpContext) =
        ctx.GetService<IKeyValueStorage>()

    let getSendMail (ctx: HttpContext) =
        ctx.GetService<ISendMailProvider>().GetSendMail()


