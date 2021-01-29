namespace PRR.Domain.Auth.ResetPasswordConfirm

open PRR.Domain.Models
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Data = { Token: string; Password: string }
    
    type Env =
        { DataContext: DbDataContext
          Logger: ILogger
          PasswordSalter: StringSalter
          KeyValueStorage: IKeyValueStorage }

    type ResetPasswordConfirm = Env -> Data -> Task<unit>
