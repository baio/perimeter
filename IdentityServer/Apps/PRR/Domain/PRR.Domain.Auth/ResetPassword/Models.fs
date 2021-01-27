namespace PRR.Domain.Auth.ResetPassword

open Common.Domain.Models
open DataAvail.KeyValueStorage.Core.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

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
    type Data = { Email: Email }

    type ResetPassword = Env -> Data -> Task<unit>
