namespace PRR.Domain.Auth.SignUp

open Common.Domain.Models.Exceptions
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.System.Models

[<AutoOpen>]
module SignUp =

    let private validatePassword =
        // https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/password-must-meet-complexity-requirements
        // Uppercase letters of European languages (A through Z, with diacritic marks, Greek and Cyrillic characters)
        // Lowercase letters of European languages (a through z, sharp-s, with diacritic marks, Greek and Cyrillic characters)
        // Base 10 digits (0 through 9)
        // Non-alphanumeric characters (special characters): (~!@#$%^&*_-+=`|\(){}[]:;"'<>,.?/) Currency symbols such as the Euro or British Pound are not counted as special characters for this policy setting.
        ()

    let signUpValidate (data: Data) =
        [| (validateNullOrEmpty "firstName" data.FirstName)
           (validateNullOrEmpty "lastName" data.LastName)
           (validateNullOrEmpty "email" data.Email) |]
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
