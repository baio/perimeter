﻿namespace PRR.Domain.Auth.LogIn

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
          CodeExpiresIn: int<minutes> }

    type LogIn = Env -> Data -> Task<Result * Events>