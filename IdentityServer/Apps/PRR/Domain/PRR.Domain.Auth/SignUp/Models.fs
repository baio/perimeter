namespace PRR.Domain.Auth.SignUp

open Common.Domain.Models
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.Domain.Auth.Common.KeyValueModels
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type OnSuccess = SignUpKV -> Task<unit>

    type Env =
        { TokenExpiresIn: int<minutes>
          DataContext: DbDataContext
          HashProvider: HashProvider
          Logger: ILogger
          PasswordSalter: StringSalter
          KeyValueStorage: IKeyValueStorage
          SendMail: SendMail }

    [<CLIMutable>]
    type Data =
        { FirstName: string
          LastName: string
          Email: string
          Password: string
          QueryString: string }

    type SignUp = Env -> Data -> Task<unit>
