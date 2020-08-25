﻿namespace PRR.Domain.Auth.LogOut

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data =
        { AccessToken: Token
          ReturnUri: string }

    type Result =
        { ReturnUri: string }

    type Env =
        { DataContext: DbDataContext
          AccessTokenSecret: string }

    type LogOut = Env -> Data -> Task<Result * Events>