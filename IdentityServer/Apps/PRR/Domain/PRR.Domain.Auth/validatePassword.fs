namespace PRR.Domain.Auth.Utils

open Common.Domain.Models
open Common.Domain.Models.Exceptions
open Common.Domain.Utils
open Models
open PRR.System.Models

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
            let validate = fun a b -> validateRegex "password" a b password
            [| validateNullOrEmpty "password" password
               validateMinLength 6 "password" password
               validateMaxLength 100 "password" password
               validate MISS_UPPER_LETTER "[A-Z]"
               validate MISS_LOWER_LETTER "[a-z]"
               validate MISS_SPECIAL_CHAR @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"
               validate MISS_DIGIT "[0-9]" |]
