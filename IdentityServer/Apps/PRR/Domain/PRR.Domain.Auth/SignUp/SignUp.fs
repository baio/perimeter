﻿namespace PRR.Domain.Auth.SignUp

open System.Diagnostics
open Akkling
open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Domain.Auth.Utils
open PRR.System.Models
open Microsoft.Extensions.Logging

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
            
            Activity.Current.AddTag("user.email", data.Email)

            let dataContext = env.DataContext

            task {
                
                // check user this the same email not exists
                let! sameEmailUsersCount = query {
                                               for user in dataContext.Users do
                                                   where (user.Email = data.Email)
                                                   select user.Id
                                           }
                                           |> toCountAsync

                if sameEmailUsersCount > 0 then return raise (Conflict(ConflictErrorField("name", UNIQUE)))

#if E2E
                let hash = "HASH"
#else
                let hash = env.HashProvider()
#endif

                return { FirstName = data.FirstName
                         LastName = data.LastName
                         Token = hash
                         Password = data.Password
                         Email = data.Email
                         QueryString =
                             if System.String.IsNullOrEmpty data.QueryString then None
                             else (Some(data.QueryString.TrimStart('?'))) }
                       |> UserSignedUpEvent
            }
