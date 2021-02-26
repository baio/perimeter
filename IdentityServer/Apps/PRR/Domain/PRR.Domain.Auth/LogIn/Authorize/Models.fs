namespace PRR.Domain.Auth.LogIn

open PRR.Domain.Models
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Result =
        { State: string
          Code: string
          RedirectUri: string }

    [<CLIMutable>]
    type Data =
        { Client_Id: ClientId
          Response_Type: string
          State: string
          Redirect_Uri: Uri
          Scope: Scope
          Email: string
          Password: string
          Code_Challenge: string
          Code_Challenge_Method: string }

    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          PasswordSalter: StringSalter
          CodeExpiresIn: int<minutes>
          SSOExpiresIn: int<minutes>
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type LogIn = Env -> string option -> Data -> Task<Result>
