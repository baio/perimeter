module PRR.Domain.Auth.LogInEmail

open Common.Domain.Models
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open PRR.Domain.Auth.LogInToken

let private getUserDataForToken (dataContext: DbDataContext) email password =
    getUserDataForToken' dataContext <@ fun (user: User) -> user.Email = email && user.Password = password @>


// Exclusively for e2e test
let logInEmail (env: Env) clientId email password =
    task {
        match! getUserDataForToken env.DataContext email password with
        | Some tokenData ->
            let! (clientId, _) = PRR.Domain.Auth.LogIn.UserHelpers.getClientIdAndIssuer env.DataContext clientId email
            let! validatedScopes = validateScopes env.DataContext email clientId [||]
            let! result = signInUser env tokenData clientId validatedScopes
            return result
        | None -> return! raiseTask (unAuthorized "user is not found")
    }
