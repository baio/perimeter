namespace PRR.Domain.Auth.LogIn.Authorize

open PRR.Domain.Auth.LogIn.Common

[<AutoOpen>]
module Authorize =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open PRR.Domain.Auth.Common
    open Microsoft.Extensions.Logging
    open DataAvail.Http.Exceptions

    let validateData isPKCE (data: AuthorizeData) =
        let scope =
            if data.Scope = null then "" else data.Scope

        let pkceValidations =
            match isPKCE with
            | true ->
                [| validateNullOrEmpty "code_challenge" data.Code_Challenge
                   validateNullOrEmpty "code_challenge_method" data.Code_Challenge_Method
                   validateContains [| "S256" |] "code_challenge_method" data.Code_Challenge_Method |]
            | false -> [||]

        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "response_type" data.Response_Type)
           (validateContains [| "code" |] "response_type" data.Response_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNullOrEmpty "scope" scope)
           (validateContainsAll [| "openid" |] "scope" (scope.Split " "))
           (validateNullOrEmpty "email" data.Email)
           (validateEmail "email" data.Email)
           (validateNullOrEmpty "password" data.Password) |]
        |> Array.append pkceValidations
        |> mapBadRequest


    let authorize: Authorize =
        fun env sso data ->

            env.Logger.LogDebug("LogIn with ${@data} and ${sso}", data, sso.IsSome)

            let isPKCE = isNotEmpty data.Code_Challenge

            env.Logger.LogDebug("Is PKCE ${isPKCE}", isPKCE)

            match validateData isPKCE data with
            | Some ex ->
                env.Logger.LogWarning("Data validation failed {@ex}", ex)
                raise ex
            | None -> ()

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
                          RedirectUri = data.Redirect_Uri
                          Scope = data.Scope
                          Email = data.Email
                          CodeChallenge = data.Code_Challenge
                          CodeChallengeMethod = data.Code_Challenge_Method }

                    let! result = logInUser env sso loginData None

                    return result

                | None ->
                    env.Logger.LogWarning("Wrong email or password")
                    return! raise (unAuthorized "Wrong email or password")
            }
