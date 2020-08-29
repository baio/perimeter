namespace PRR.Domain.Auth.LogInSSO

open Common.Domain.Models

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
          CodeExpiresIn: int<minutes> }

    type LogInSSO = Env -> Data -> SSO.Item -> Task<PRR.Domain.Auth.LogIn.Models.Result * Events>
