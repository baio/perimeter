namespace PRR.Domain.Auth.SignUp

open Common.Domain.Models
open Common.Domain.Models.Exceptions
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.System.Models

[<AutoOpen>]
module SignUp =

    let private validatePassword password =
        match validateNullOrEmpty "password" password with
        | Some err -> [| Some err |]
        | None ->
            // https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/password-must-meet-complexity-requirements
            // A through Z
            // a through z
            // 0 through 9
            // Non-alphanumeric characters (special characters): (~!@#$%^&*_-+=`|\(){}[]:;"'<>,.?/) Currency symbols such as the Euro or British Pound are not counted as special characters for this policy setting.
            let validate = fun a b -> validateRegex "password" a b password
            [| validateNullOrEmpty "password" password
               validateMinLength 6 "password" password
               validateMaxLength 100 "password" password
               validate MISS_UPPER_LETTER "[A-Z]"
               validate MISS_LOWER_LETTER "[a-z]"
               validate MISS_SPECIAL_CHAR @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"
               validate MISS_DIGIT "[0-9]" |]

    let signUpValidate (data: Data) =
        [| (validateNullOrEmpty "firstName" data.FirstName)
           (validateNullOrEmpty "lastName" data.LastName)
           (validateNullOrEmpty "email" data.Email) |]
        |> Array.append (validatePassword data.Password)
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
