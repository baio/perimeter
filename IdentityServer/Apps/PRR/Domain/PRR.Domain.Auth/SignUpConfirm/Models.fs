namespace PRR.Domain.Auth.SignUpConfirm

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
        { Password: string
          Token: string }

    type SignUpConfirm = Env -> SignUpToken.Item -> Data -> Task<Events>
