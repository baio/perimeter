namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

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

    type Result =
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

    type LogIn = Env -> string option -> Data -> Task<Result>
