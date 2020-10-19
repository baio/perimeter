namespace PRR.Domain.Auth.Social

open Common.Domain.Models
open PRR.Data.DataContext

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data =
        { ClientId: string
          SocialName: string }

    type Env =
        { DataContext: DbDataContext
          HashProvider: HashProvider }
