﻿namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models

open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.System.Models
open System

[<AutoOpen>]
module Authorize =

    let validateData (data: Data): BadRequestError array =
        let scope =
            if data.Scope = null then ""
            else data.Scope
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "response_type" data.Response_Type)
           (validateContains [| "code" |] "response_type" data.Response_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNullOrEmpty "scope" scope)
           (validateContainsAll [| "openid"; "profile" |] "scope" (scope.Split " "))
           (validateNullOrEmpty "email" data.Email)
           (validateEmail "email" data.Email)
           (validateNullOrEmpty "password" data.Password)
           (validateNullOrEmpty "code_challenge" data.Code_Challenge)
           (validateNullOrEmpty "code_challenge_method" data.Code_Challenge_Method)
           (validateContains [| "S256" |] "code_challenge_method" data.Code_Challenge_Method) |]
        |> Array.choose id

    let logIn: LogIn =
        fun sso env data ->
            let dataContext = env.DataContext
            task {

                let! clientId = getClientId env.DataContext data.Client_Id data.Email

                let! app = query {
                               for app in dataContext.Applications do
                                   where (app.ClientId = clientId)
                                   select
                                       (Tuple.Create(app.SSOEnabled, app.AllowedCallbackUrls, app.Domain.Pool.TenantId))
                           }
                           |> toSingleExnAsync (unAuthorized ("client_id not found"))
                let (ssoEnabled, callbackUrls, tenantId) = app
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

                    let loginItem: LogIn.Item =
                        { Code = code
                          ClientId = data.Client_Id
                          CodeChallenge = data.Code_Challenge
                          Scopes = (data.Scope.Split " ")
                          UserId = userId
                          ExpiresAt = expiresAt
                          RedirectUri = data.Redirect_Uri }


                    let ssoItem =
                        match ssoEnabled, sso with
                        | (true, Some sso) ->
                            Some
                                ({ Code = sso
                                   UserId = userId
                                   TenantId = tenantId
                                   ExpiresAt = expiresAt
                                   Email = data.Email }: SSO.Item)
                        // TODO : Handle case SSO enabled but sso token not found
                        | _ -> None

                    let evt = UserLogInSuccessEvent(loginItem, ssoItem)

                    return (result, evt)

                | None ->
                    return! raise (unAuthorized "Wrong email or password")
            }
