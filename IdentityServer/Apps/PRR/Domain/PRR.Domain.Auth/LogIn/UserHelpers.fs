namespace PRR.Domain.Auth.LogIn

open System
open System.Threading.Tasks
open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Domain.Auth
open System.Linq
open PRR.Domain.Auth.Common
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

[<AutoOpen>]
module internal UserHelpers =

    let private DEFAULT_CLIENT_ID = "__DEFAULT_CLIENT_ID__"

    let getUserId (dataContext: DbDataContext) (email, password) =
        query {
            for user in dataContext.Users do
                where (user.Email = email && user.Password = password)
                select user.Id
        }
        |> toSingleOptionAsync




