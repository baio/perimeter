namespace PRR.Domain.Auth.LogInSSO

open PRR.Domain.Models

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks
open PRR.Domain.Auth.LogIn.Authorize

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data =
        { Client_Id: ClientId
          Response_Type: string
          State: string
          Redirect_Uri: Uri
          Scope: Scope
          Code_Challenge: string
          Code_Challenge_Method: string
          Prompt: string option }


    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          CodeExpiresIn: int<minutes>
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type LogInSSO = Env -> string -> Data -> Task<Result>
