namespace PRR.Domain.Auth.LogIn

open PRR.Data.DataContext
open DataAvail.EntityFramework.Common

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




