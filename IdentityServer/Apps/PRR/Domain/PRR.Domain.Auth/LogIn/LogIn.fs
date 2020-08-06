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

    let validateDefaultClientData (data: DefaultClientData): BadRequestError array =
        { Client_Id = "123"
          Response_Type = data.Response_Type
          State = data.State
          Redirect_Uri = data.Redirect_Uri
          Scopes = data.Scopes
          Email = data.Email
          Password = data.Password
          Code_Challenge = data.Code_Challenge
          Code_Challenge_Method = data.Code_Challenge_Method }
        |> validateData


    let logIn: LogIn =
        fun env data ->
            let dataContext = env.DataContext
            task {
                let! callbackUrls = query {
                                        for app in dataContext.Applications do
                                            where (app.ClientId = data.Client_Id)
                                            select app.AllowedCallbackUrls
                                    }
                                    |> toSingleExnAsync (unAuthorized ("client_id not found"))
                if (callbackUrls <> "*" && (callbackUrls.Split(",")
                                            |> Seq.map (fun x -> x.Trim())
                                            |> Seq.contains data.Redirect_Uri
                                            |> not))
                then return! raise (unAuthorized "return_uri mismatch")
                let saltedPassword = env.PasswordSalter data.Password
                match! getUserId dataContext (data.Email, saltedPassword) with
                | Some userId ->
                    let code = env.CodeGenerator()

                    let result: Result =
                        { RedirectUri = data.Redirect_Uri
                          State = data.State
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
                    return! raise UnAuthorized'
            }

    let defaultClientLogIn: DefaultClientLogIn =
        fun env data ->
            task {
                // TODO : User defaultDomainId
                let! managmentAppClientId = query {
                                                for app in env.DataContext.Applications do
                                                    where (app.Domain.Tenant.User.Email = data.Email)
                                                    select app.ClientId
                                            }
                                            |> LinqHelpers.toSingleExnAsync
                                                (unAuthorized "Tenant's management API is not found")

                return! logIn env
                            { Client_Id = managmentAppClientId
                              Response_Type = data.Response_Type
                              State = data.State
                              Redirect_Uri = data.Redirect_Uri
                              Scopes = data.Scopes
                              Email = data.Email
                              Password = data.Password
                              Code_Challenge = data.Code_Challenge
                              Code_Challenge_Method = data.Code_Challenge_Method }
            }
