namespace PRR.Domain.Auth.LogInSSO

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext

[<AutoOpen>]
module internal UserHelpers =
    
    let getUserId (dataContext: DbDataContext) (email) =
        query {
            for user in dataContext.Users do
                where (user.Email = email)
                select user.Id
        }
        |> toSingleOptionAsync
   