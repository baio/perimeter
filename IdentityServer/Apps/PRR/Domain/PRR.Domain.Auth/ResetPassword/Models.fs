namespace PRR.Domain.Auth.ResetPassword

open PRR.Domain.Models
open DataAvail.KeyValueStorage.Core.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks
open PRR.Domain.Auth.Common

[<AutoOpen>]
module Models =

    type Env =
        { DataContext: DbDataContext
          Logger: ILogger
          SendMail: SendMail
          KeyValueStorage: IKeyValueStorage
          TokenExpiresIn: int<minutes>
          HashProvider: HashProvider }

    [<CLIMutable>]
    type Data =
        { Email: Email
          QueryString: string }

    type ResetPassword = Env -> Data -> Task<unit>
