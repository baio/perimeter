namespace PRR.Domain.Auth.SignUp

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =
    type Env =
        { DataContext: DbDataContext
          HashProvider: HashProvider }

    [<CLIMutable>]
    type Data =
        { FirstName: string
          LastName: string
          Email: string }

    type SignUp = Env -> Data -> Task<Events>
