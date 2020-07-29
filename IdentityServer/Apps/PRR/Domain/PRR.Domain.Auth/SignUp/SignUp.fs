namespace PRR.Domain.Auth.SignUp

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Domain.Auth.Utils
open PRR.System.Models

[<AutoOpen>]
module SignUp =

    let validateData (data: Data) =
        [| (validateNullOrEmpty "firstName" data.FirstName)
           (validateNullOrEmpty "lastName" data.LastName)
           (validateNullOrEmpty "email" data.Email) |]
        |> Array.append (Utils.validatePassword data.Password)
        |> Array.choose id

    let signUp: SignUp =
        fun env data ->

            let dataContext = env.DataContext

            task {

                // check user this the same email not exists
                let! sameEmailUsersCount = query {
                                               for user in dataContext.Users do
                                                   where (user.Email = data.Email)
                                                   select user.Id
                                           }
                                           |> toCountAsync

                if sameEmailUsersCount > 0 then return raise (Conflict "User with the same email already exist")

                let hash = env.HashProvider()

                return { FirstName = data.FirstName
                         LastName = data.LastName
                         Token = hash
                         Password = data.Password
                         Email = data.Email }
                       |> UserSignedUpEvent
            }
