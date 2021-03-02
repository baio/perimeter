namespace PRR.Domain.Auth.LogIn.Authorize

open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Models
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type AuthorizeResult =
        { State: string
          Code: string
          RedirectUri: string }

    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          PasswordSalter: StringSalter
          CodeExpiresIn: int<minutes>
          SSOExpiresIn: int<minutes>
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type Authorize = Env -> string option -> AuthorizeData -> Task<string>
