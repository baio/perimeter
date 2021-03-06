namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

[<AutoOpen>]
module private ValidateAuthorizeData =

    open PRR.Domain.Auth.Common
    open DataAvail.Http.Exceptions
    open PRR.Domain.Auth.LogIn.Common
    
    let validateAuthorizeData validateEmailPassword (data: AuthorizeData) =

        let isPKCE = isNotEmpty data.Code_Challenge

        let scope =
            if data.Scope = null then "" else data.Scope

        let pkceValidations =
            match isPKCE with
            | true ->
                [| validateNullOrEmpty "code_challenge" data.Code_Challenge
                   validateNullOrEmpty "code_challenge_method" data.Code_Challenge_Method
                   validateContains [| "S256" |] "code_challenge_method" data.Code_Challenge_Method |]
            | false -> [||]

        let emailPasswordValidations =
            match validateEmailPassword with
            | true ->
                [| (validateNullOrEmpty "email" data.Email)
                   (validateEmail "email" data.Email)
                   (validateNullOrEmpty "password" data.Password) |]
            | false -> [||]

        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "response_type" data.Response_Type)
           (validateContains [| "code" |] "response_type" data.Response_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNullOrEmpty "scope" scope)
           (validateContainsAll [| "openid" |] "scope" (scope.Split " ")) |]
        |> Array.append pkceValidations
        |> Array.append emailPasswordValidations
        |> mapBadRequest
