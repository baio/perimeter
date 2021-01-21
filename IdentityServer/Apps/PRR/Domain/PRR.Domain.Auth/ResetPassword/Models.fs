namespace PRR.Domain.Auth.ResetPassword

open Common.Domain.Models
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type OnSuccess = string -> Task<unit>

    type Env =
        { DataContext: DbDataContext
          Logger: ILogger
          OnSuccess: OnSuccess }

    [<CLIMutable>]
    type Data = { Email: Email }

    type ResetPassword = Env -> Data -> Task<unit>
