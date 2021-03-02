namespace PRR.Domain.Auth.LogIn.Authorize

open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Models
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Authorize = AuthorizeEnv -> string option -> AuthorizeData -> Task<string>
