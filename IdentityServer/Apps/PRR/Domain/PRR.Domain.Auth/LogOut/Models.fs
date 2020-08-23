namespace PRR.Domain.Auth.LogOut

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data =
        { ReturnUri: string }

    type Result =
        { ReturnUri: string }

    type Env =
        { DataContext: DbDataContext }

    type LogOut = Env -> Data -> (UserId * ClientId) -> Task<Result * Events>
