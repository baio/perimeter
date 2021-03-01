namespace PRR.Domain.Auth.LogIn.AuthorizeSSO

open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Models

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks
open PRR.Domain.Auth.LogIn.Authorize

[<AutoOpen>]
module Models =

    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          CodeExpiresIn: int<minutes>
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type AuthorizeSSO = Env -> string -> AuthorizeData -> Task<AuthorizeResult>
