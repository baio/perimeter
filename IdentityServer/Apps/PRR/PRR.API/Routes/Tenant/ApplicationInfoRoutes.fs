namespace PRR.API.Routes.Tenant

open Giraffe
open DataAvail.Common
open DataAvail.Common.ReaderTask
open DataAvail.Giraffe.Common

open PRR.Domain.Models
open PRR.Domain.Tenant.ApplicationInfo
open PRR.API.Routes


[<AutoOpen>]
module private ApplicationInfoHandlers =

    let getApplicationInfo (clientId: ClientId) =
        wrap ((getApplicationInfo clientId) <!> getDataContext')

module ApplicationInfo =

    let createRoutes () =
        routef "/applications/%s" getApplicationInfo
