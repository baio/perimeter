module PRR.Domain.Auth.LogInEmail

open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open PRR.Domain.Auth.LogInToken
open DataAvail.Http.Exceptions

let private getUserDataForToken (dataContext: DbDataContext) email password =
    getUserDataForToken' dataContext <@ fun (user: User) -> user.Email = email && user.Password = password @> None


// Exclusively for e2e test
let logInEmail (env) clientId email password =
    task {
        match! getUserDataForToken env.DataContext email password with
        | Some tokenData ->
            let! result = signInUser env tokenData clientId (RequestedScopes [ "openid"; "profile" ])
            return result
        | None -> return raise (unAuthorized "user is not found")
    }
