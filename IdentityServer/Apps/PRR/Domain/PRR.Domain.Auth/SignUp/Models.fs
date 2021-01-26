namespace PRR.Domain.Auth.SignUp

open Common.Domain.Models
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type OnSuccess = (SignUpSuccess * System.DateTime) -> Task<unit>

    type Env =
        { TokenExpiresIn: int<minutes>
          DataContext: DbDataContext
          HashProvider: HashProvider
          Logger: ILogger
          OnSuccess: OnSuccess
          PasswordSalter: StringSalter }

    [<CLIMutable>]
    type Data =
        { FirstName: string
          LastName: string
          Email: string
          Password: string
          QueryString: string }

    type SignUp = Env -> Data -> Task<unit>
