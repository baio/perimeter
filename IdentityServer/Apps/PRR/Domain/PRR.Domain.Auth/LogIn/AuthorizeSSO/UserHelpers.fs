namespace PRR.Domain.Auth.LogIn.AuthorizeSSO

open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open DataAvail.EntityFramework.Common

[<AutoOpen>]
module internal UserHelpers =
    
    let getUserId (dataContext: DbDataContext) (email) =
        query {
            for user in dataContext.Users do
                where (user.Email = email)
                select user.Id
        }
        |> toSingleOptionAsync
   