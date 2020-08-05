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
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "response_type" data.Response_Type)
           (validateContains [| "code" |] "response_type" data.Response_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNull "scopes" data.Scopes)
           (validateContainsAll [| "openid"; "profile" |] "scopes" data.Scopes)
           (validateNullOrEmpty "email" data.Email)
           (validateEmail "email" data.Email)
           (validateNullOrEmpty "password" data.Password)
           (validateNullOrEmpty "code_challenge" data.Code_Challenge)
           (validateNullOrEmpty "code_challenge_method" data.Code_Challenge_Method)
           (validateContains [| "S256" |] "code_challenge_method" data.Code_Challenge_Method) |]
        |> Array.choose id

    let logIn: LogIn =
        fun env data ->
            let dataContext = env.DataContext
            task {
                let saltedPassword = env.PasswordSalter data.Password
                match! getUserId dataContext (data.Email, saltedPassword) with
                | Some userId ->
                    let code = env.CodeGenerator()

                    let result: Result =
                        { State = data.State
                          Code = code }

                    let expiresAt = DateTime.UtcNow.AddMinutes(float env.CodeExpiresIn)

                    let evt =
                        ({ Code = code
                           ClientId = data.Client_Id
                           CodeChallenge = data.Code_Challenge
                           Scopes = data.Scopes
                           UserId = userId
                           ExpiresAt = expiresAt
                           RedirectUri = data.Redirect_Uri }: LogIn.Item)
                        |> UserLogInSuccessEvent

                    return (result, evt)
                | None ->
                    return! raise (UnAuthorized None)
            }
