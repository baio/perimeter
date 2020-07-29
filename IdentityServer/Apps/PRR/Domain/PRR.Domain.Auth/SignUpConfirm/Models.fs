namespace PRR.Domain.Auth.SignUpConfirm

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Env =
        { DataContext: DbDataContext }
        
    type Data = {
        Token: string
    }

    type SignUpConfirm = Env -> SignUpToken.Item -> Task<Events>
