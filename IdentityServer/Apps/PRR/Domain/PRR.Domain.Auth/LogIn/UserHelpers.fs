namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Microsoft.FSharp.Linq
open Models
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

[<AutoOpen>]
module internal UserHelpers =

    let getUserId (dataContext: DbDataContext) (email, password) =
        query {
            for user in dataContext.Users do
                where (user.Email = email && user.Password = password)
                select user.Id
        }
        |> toSingleOptionAsync
