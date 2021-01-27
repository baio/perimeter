namespace PRR.Domain.Auth.Utils

open Common.Domain.Models
open Common.Domain.Utils

[<AutoOpen>]
module Utils =

    let validatePassword password =
        match validateNullOrEmpty "password" password with
        | Some err -> [| Some err |]
        | None ->
            // https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/password-must-meet-complexity-requirements
            // A through Z
            // a through z
            // 0 through 9
            // Non-alphanumeric characters (special characters): (~!@#$%^&*_-+=`|\(){}[]:;"'<>,.?/) Currency symbols such as the Euro or British Pound are not counted as special characters for this policy setting.
            // number || special character ||upper letter
            let validate =
                fun a b -> validateRegex "password" a b password

            let validatedPassword =
                [| validate MISS_UPPER_LETTER "[A-Z]"
                   validate MISS_SPECIAL_CHAR @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"
                   validate MISS_DIGIT "[0-9]" |]
                |> Seq.forall (fun x -> x.IsSome)
                |> function
                | true -> Some(BadRequestFieldError("password", PASSWORD))
                | false -> None
                
            [| validateNullOrEmpty "password" password
               validateMinLength 6 "password" password
               validateMaxLength 100 "password" password
               validate PASSWORD "[a-z]"
               validatedPassword |]
