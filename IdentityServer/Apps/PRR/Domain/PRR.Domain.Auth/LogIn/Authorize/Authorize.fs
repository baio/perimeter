namespace PRR.Domain.Auth.LogIn.Authorize

open PRR.Domain.Auth.LogIn.Common
open DataAvail.Common

[<AutoOpen>]
module Authorize =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open PRR.Domain.Auth.Common
    open Microsoft.Extensions.Logging
    open DataAvail.Http.Exceptions

    let validateData validateEmailPassword (data: AuthorizeData) =

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

    let private redirectToLoginPageUrl data =

        let qs =
            [| mapNullValue "client_id" data.Client_Id
               mapOptionValue "prompt" data.Prompt
               mapNullValue "scope" data.Scope
               mapNullValue "state" data.State
               mapNullValue "nonce" data.Nonce
               mapNullValue "code_challenge" data.Code_Challenge
               mapNullValue "redirect_uri" data.Redirect_Uri
               mapNullValue "response_type" data.Response_Type
               mapNullValue "code_challenge_method" data.Code_Challenge_Method |]
            |> Seq.choose id
            |> joinQueryStringTuples

        sprintf "http://localhost:4200?%s" qs

    let private tryLogIn: Authorize =
        fun env sso data ->
            let dataContext = env.DataContext

            task {
                let saltedPassword = env.PasswordSalter data.Password

                match! getUserId dataContext (data.Email, saltedPassword) with
                | Some userId ->

                    let loginData: LoginData =
                        { UserId = userId
                          ClientId = data.Client_Id
                          ResponseType = data.Response_Type
                          State = data.State
                          Nonce = data.Nonce
                          RedirectUri = data.Redirect_Uri
                          Scope = data.Scope
                          Email = data.Email
                          CodeChallenge = data.Code_Challenge
                          CodeChallengeMethod = data.Code_Challenge_Method }

                    let! result = logInUser env sso loginData None

                    let redirectUrlSuccess =
                        sprintf "%s?code=%s&state=%s" result.RedirectUri result.Code result.State

                    return redirectUrlSuccess

                | None ->
                    env.Logger.LogWarning("Wrong email or password")
                    return! raise (unAuthorized "Wrong email or password")

            }

    let authorize: Authorize =
        fun env sso data ->

            env.Logger.LogDebug("LogIn with ${@data} and ${sso}", data, sso.IsSome)

            let emailPasswordEmpty =
                (isEmpty data.Email) && (isEmpty data.Password)

            match validateData (not emailPasswordEmpty) data with
            | Some ex ->
                env.Logger.LogWarning("Data validation failed {@ex}", ex)
                raise ex
            | None -> ()

            task {
                if emailPasswordEmpty then
                    env.Logger.LogDebug("Email and password are empty redirect to login page")
                    return redirectToLoginPageUrl data
                else
                    env.Logger.LogDebug("Email and password defined trying to login")
                    return! tryLogIn env sso data
            }
