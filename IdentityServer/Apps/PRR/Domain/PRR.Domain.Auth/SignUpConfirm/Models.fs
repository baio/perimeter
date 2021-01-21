namespace PRR.Domain.Auth.SignUpConfirm

open Common.Domain.Models
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks
open PRR.System.Models.SignUpToken

[<AutoOpen>]
module Models =

    type Data = { Token: string }
    type OnSuccess = SignUpConfirmSuccess -> Task<unit>
    type GetTokenItem = string -> Task<Item option>

    type Env =
        { DataContext: DbDataContext
          Logger: ILogger
          OnSuccess: OnSuccess
          GetTokenItem: GetTokenItem }

    type SignUpConfirm = Env -> Data -> Task<unit>
