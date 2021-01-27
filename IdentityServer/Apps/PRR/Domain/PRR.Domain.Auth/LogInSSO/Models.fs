namespace PRR.Domain.Auth.LogInSSO

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
          Code_Challenge: string
          Code_Challenge_Method: string
          Prompt: string option }


    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          CodeExpiresIn: int<minutes>
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type LogInSSO = Env -> string -> Data -> Task<PRR.Domain.Auth.LogIn.Models.Result>
