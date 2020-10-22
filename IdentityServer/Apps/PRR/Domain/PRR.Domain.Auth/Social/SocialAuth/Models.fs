namespace PRR.Domain.Auth.Social

open Common.Domain.Models
open PRR.Data.DataContext

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data =
        { Social_Name: string
          Client_Id: ClientId
          Response_Type: string
          State: string
          Redirect_Uri: Uri
          Scope: Scope
          Code_Challenge: string
          Code_Challenge_Method: string }

    type Env =
        { DataContext: DbDataContext
          HashProvider: HashProvider
          SocialCallbackUrl: string }
