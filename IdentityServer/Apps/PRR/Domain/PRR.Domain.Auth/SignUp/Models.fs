namespace PRR.Domain.Auth.SignUp

open Common.Domain.Models
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type OnSuccess = SignUpSuccess -> Task<unit>

    type Env =
        { DataContext: DbDataContext
          HashProvider: HashProvider
          Logger: ILogger
          OnSuccess: OnSuccess }

    [<CLIMutable>]
    type Data =
        { FirstName: string
          LastName: string
          Email: string
          Password: string
          QueryString: string }

    type SignUp = Env -> Data -> Task<unit>
