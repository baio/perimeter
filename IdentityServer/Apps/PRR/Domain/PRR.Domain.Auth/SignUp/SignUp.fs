namespace PRR.Domain.Auth.SignUp

open System.Diagnostics
open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Domain.Auth.Common
open PRR.Domain.Auth.Utils
open Microsoft.Extensions.Logging
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

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
                let! userResult =
                    query {
                        for user in dataContext.Users do
                            where (user.Email = data.Email)
                            select (user.Id, user.Password)
                    }
                    |> toSingleOptionAsync

                let existentUserId =
                    match userResult with
                    | None -> None
                    | Some (userId, password) ->
                        if password <> null then
                            let ex =
                                Conflict(ConflictErrorField("name", UNIQUE))

                            env.Logger.LogWarning
                                ("User with the same email already exists {email} {@ex}", data.Email, ex)

                            raise ex
                        else
                            env.Logger.LogDebug
                                ("User with the same email already exists {email} but password is null since it was registered from some social provider",
                                 data.Email)

                            Some userId

#if E2E
                let token = "HASH"
#else
                let token = env.HashProvider()

#endif
                let encodedPassword = env.PasswordSalter data.Password

                let queryString =
                    if System.String.IsNullOrEmpty data.QueryString
                    then None
                    else (Some(data.QueryString.TrimStart('?')))

                let expiresAt =
                    System.DateTime.UtcNow.AddMinutes(float (int env.TokenExpiresIn))

                let kv: SignUpKV =
                    { FirstName = data.FirstName
                      LastName = data.LastName
                      Email = data.Email
                      Password = encodedPassword
                      Token = token
                      ExistentUserId = existentUserId
                      ExpiredAt = expiresAt
                      QueryString = queryString }

                env.Logger.LogInformation
                    ("Signup success data {@data} and expires at ${expiresAt}",
                     { kv with
                           Password = "***"
                           Token = "***" },
                     expiresAt)

                do! onSuccess env kv
            }
