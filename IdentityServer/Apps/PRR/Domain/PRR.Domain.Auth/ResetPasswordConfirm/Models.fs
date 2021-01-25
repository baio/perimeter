namespace PRR.Domain.Auth.ResetPasswordConfirm

open Common.Domain.Models
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks
open PRR.System.Models.ResetPassword

[<AutoOpen>]
module Models =

    type Data = { Token: string; Password: string }
    type OnSuccess = Email -> Task<unit>
    type GetTokenItem = string -> Task<Email option>

    type Env =
        { DataContext: DbDataContext
          Logger: ILogger
          PasswordSalter: StringSalter
          OnSuccess: OnSuccess
          GetTokenItem: GetTokenItem }

    type ResetPasswordConfirm = Env -> Data -> Task<unit>
