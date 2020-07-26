namespace PRR.Domain.Auth.ResetPasswordConfirm

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Env =
        { DataContext: DbDataContext
          PasswordSalter: StringSalter }

    [<CLIMutable>]
    type Data =
        {
            Token: string
            Password: string
        }

    type ResetPasswordConfirm = Env -> ResetPassword.Item -> Data -> Task<Events>
