namespace PRR.Domain.Auth.ResetPassword

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Env = DbDataContext

    [<CLIMutable>]
    type Data =
        { Email: Email }

    type ResetPassword = Env -> Data -> Task<Events>
