namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Common
open DataAvail.Common.ReaderTask
open DataAvail.Giraffe.Common

open PRR.Domain.Models
open PRR.Domain.Auth.ApplicationInfo

module internal GetApplicationInfo =

    let handler (clientId: ClientId) =
        wrap ((getApplicationInfo clientId) <!> getDataContext')

