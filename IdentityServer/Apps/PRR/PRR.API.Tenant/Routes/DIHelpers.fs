namespace PRR.API.Tenant.Routes

open PRR.API.Common.Infra
open PRR.API.Tenant.Configuration
open PRR.Data.DataContext
open Giraffe
open Microsoft.AspNetCore.Http
open DataAvail.Common.ReaderTask
open PRR.API.Tenant.Infra

[<AutoOpen>]
module DIHelpers =
    let getDataContext (ctx: HttpContext) = ctx.GetService<DbDataContext>()

    let getDataContext': ReaderTask<HttpContext, DbDataContext> = ofReader getDataContext

    let getConfig (ctx: HttpContext) =
        ctx
            .GetService<IConfigProvider<AppConfig>>()
            .GetConfig()

    let getViewsDb (ctx: HttpContext) = ctx.GetService<IViewsDbProvider>().Db

    let getLogger (ctx: HttpContext) = ctx.GetLogger()

    let getAuthStringsGetter (ctx: HttpContext) =
        ctx.GetService<IAuthStringsGetterProvider>().AuthStringsGetter
