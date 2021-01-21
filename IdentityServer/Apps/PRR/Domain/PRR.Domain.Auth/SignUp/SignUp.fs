namespace PRR.Domain.Auth.SignUp

open System.Diagnostics
open Akkling
open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Domain.Auth.Common
open PRR.Domain.Auth.Utils
open PRR.System.Models
open Microsoft.Extensions.Logging

[<AutoOpen>]
module SignUp =

    let private validateData (data: Data) =

        [| (validateNullOrEmpty "firstName" data.FirstName)
           (validateNullOrEmpty "lastName" data.LastName)
           (validateNullOrEmpty "email" data.Email) |]
        |> Array.append (validatePassword data.Password)
        |> mapBadRequest

    let signUp: SignUp =
        fun env data ->

            env.Logger.LogInformation("Signup with data {@data}", { data with Password = "***" })

            Activity.Current.AddTag("user.email", data.Email)
            |> ignore

            match validateData data with
            | Some ex ->
                env.Logger.LogWarning("Data validation failed {@ex}", ex)
                raise ex
            | None -> ()

            let dataContext = env.DataContext

            task {

                // check user with the same email not exists
                let! sameEmailUsersCount =
                    query {
                        for user in dataContext.Users do
                            where (user.Email = data.Email)
                            select user.Id
                    }
                    |> toCountAsync

                if sameEmailUsersCount > 0 then
                    let ex =
                        Conflict(ConflictErrorField("name", UNIQUE))

                    env.Logger.LogWarning("User with the same email already exists {email} {@ex}", data.Email, ex)
                    raise ex

#if E2E
                let token = "HASH"
#else
                let token = env.HashProvider()
#endif

                let signupSuccessData =
                    { FirstName = data.FirstName
                      LastName = data.LastName
                      Token = token
                      Password = data.Password
                      Email = data.Email
                      QueryString =
                          if System.String.IsNullOrEmpty data.QueryString
                          then None
                          else (Some(data.QueryString.TrimStart('?'))) }

                env.Logger.LogInformation
                    ("Signup success data {@data}",
                     { signupSuccessData with
                           Password = "***"
                           Token = "***" })

                do! env.OnSuccess signupSuccessData
            }
