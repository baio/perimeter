namespace PRR.API.Routes.Tenant

open Giraffe
open Common.Domain.Giraffe
open Common.Domain.Models
open PRR.Domain.Tenant.ApplicationInfo
open PRR.API.Routes
open Common.Utils.ReaderTask

[<AutoOpen>]
module private ApplicationInfoHandlers =

    let getApplicationInfo (clientId: ClientId) =
        wrap ((getApplicationInfo clientId) <!> getDataContext')

module ApplicationInfo =

    let createRoutes () =
        routef "/applications/%s" getApplicationInfo
