namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models

open Common.Domain.Models.BadRequestErrors
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models
open System

[<AutoOpen>]
module Authorize =

    let validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.ClientId)
           (validateNullOrEmpty "response_type" data.ResponseType)
           (validateContains [| "code" |] "response_type" data.ResponseType)
           (validateNullOrEmpty "redirect_uri" data.RedirectUri)
           (validateUrl "redirect_uri" data.RedirectUri)
           (validateContainsAll [| "openid"; "profile" |] "scopes" data.Scopes)
           (validateNullOrEmpty "email" data.Email)
           (validateEmail "email" data.Email)
           (validateNullOrEmpty "password" data.Password)
           (validateNullOrEmpty "code_challenge" data.CodeChallenge)
           (validateNullOrEmpty "code_challenge_method" data.CodeChallengeMethod) |]
        |> Array.choose id

    let logIn: LogIn =
        fun env data ->
            let dataContext = env.DataContext
            task {
                let saltedPassword = env.PasswordSalter data.Password
                match! getUserId dataContext (data.Email, saltedPassword) with
                | Some _ ->
                    let code = env.CodeGenerator()

                    let result: Result =
                        { State = data.State
                          Code = code }

                    let expiresAt = DateTime.UtcNow.AddMinutes(float env.CodeExpiresIn)

                    let evt =
                        ({ Code = code
                           ClientId = data.ClientId
                           CodeChallenge = data.CodeChallenge
                           ExpiresAt = expiresAt }: LogIn.Item)
                        |> UserLogInSuccessEvent

                    return (result, evt)
                | None ->
                    return! raise UnAuthorized
            }
