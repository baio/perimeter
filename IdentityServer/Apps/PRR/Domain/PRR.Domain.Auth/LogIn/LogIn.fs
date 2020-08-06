namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models

open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.System.Models
open System

[<AutoOpen>]
module Authorize =

    let private DEFAULT_CLIENT_ID = "__DEFAULT_CLIENT_ID__"

    let validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "response_type" data.Response_Type)
           (validateContains [| "code" |] "response_type" data.Response_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNull "scope" data.Scope)
           (validateContainsAll [| "openid"; "profile" |] "scope" data.Scope)
           (validateNullOrEmpty "email" data.Email)
           (validateEmail "email" data.Email)
           (validateNullOrEmpty "password" data.Password)
           (validateNullOrEmpty "code_challenge" data.Code_Challenge)
           (validateNullOrEmpty "code_challenge_method" data.Code_Challenge_Method)
           (validateContains [| "S256" |] "code_challenge_method" data.Code_Challenge_Method) |]
        |> Array.choose id

    let private getClientId (dataContext: DbDataContext) clientId email =
        task {
            if clientId = DEFAULT_CLIENT_ID then
                return! query {
                            for app in dataContext.Applications do
                                where (app.Domain.Tenant.User.Email = email)
                                select app.ClientId
                        }
                        |> LinqHelpers.toSingleExnAsync (unAuthorized "Tenant's management API is not found")
            else
                return clientId
        }

    let logIn: LogIn =
        fun env data ->
            let dataContext = env.DataContext
            task {

                let! clientId = getClientId env.DataContext data.Client_Id data.Email

                let! callbackUrls = query {
                                        for app in dataContext.Applications do
                                            where (app.ClientId = clientId)
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
                           Scopes = data.Scope
                           UserId = userId
                           ExpiresAt = expiresAt
                           RedirectUri = data.Redirect_Uri }: LogIn.Item)
                        |> UserLogInSuccessEvent

                    return (result, evt)
                | None ->
                    return! raise UnAuthorized'
            }
