﻿namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Common.Domain.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Models
open PRR.Data.DataContext
open PRR.System.Models
open System
open System.Security.Cryptography
open System.Text.Encodings.Web
open System.Text.RegularExpressions

[<AutoOpen>]
module LogInToken =

    let private replace (pat: string) (rep: string) (str: string) = Regex.Replace(str, pat, rep)

    // cleanup sample  https://auth0.com/docs/flows/call-your-api-using-the-authorization-code-flow-with-pkce#javascript-sample
    let cleanupCodeChallenge =
        replace "\+" "-"
        >> replace "\/" "_"
        >> replace "=" ""

    let validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "code" data.Code)
           (validateNullOrEmpty "grant_type" data.Grant_Type)
           (validateContains [| "code" |] "grant_ype" data.Grant_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNullOrEmpty "code_verifier" data.Code_Verifier) |]
        |> Array.choose id

    let private getSuccessData (dataContext: DbDataContext) clientId userId =
        task {
            let! (domainId, appIdentifier) =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)
                        select (app.Domain.Id, app.Name)
                }
                |> toSingleExnAsync (Unexpected')


            let! userEmail =
                query {
                    for user in dataContext.Users do
                        where (user.Id = userId)
                        select user.Email
                }
                |> toSingleExnAsync (Unexpected')

            let successData: LogIn.LoginSuccessData =
                { DomainId = domainId
                  AppIdentifier = appIdentifier
                  UserEmail = userEmail
                  Date = DateTime.UtcNow }

            return successData
        }


    let logInToken: LogInToken =
        fun env item data ->
            let dataContext = env.DataContext
            if (item.ExpiresAt < DateTime.UtcNow) then raise (unAuthorized "code expires")
            if data.Client_Id <> item.ClientId
            then raise (unAuthorized "client_id mismatch")
            if data.Redirect_Uri <> item.RedirectUri
            then raise (unAuthorized "redirect_uri mismatch")

            let codeChallenge =
                env.Sha256Provider(data.Code_Verifier)
                |> cleanupCodeChallenge

            let itemCodeChallenge = item.CodeChallenge
            printfn "****"
            printfn "Code_Verifier %s" data.Code_Verifier
            printfn "1 codeChallenge %s" codeChallenge
            printfn "2 codeChallenge %s" itemCodeChallenge
            printfn "****"
            if codeChallenge <> itemCodeChallenge
            then raise (unAuthorized "code_verifier code_challenge mismatch")
            task {
                match! getUserDataForToken dataContext item.UserId with
                | Some tokenData ->
                    let! (result, clientId, isPerimeterClient) =
                        signInUser env tokenData data.Client_Id (ValidatedScopes item.ValidatedScopes)

                    let refreshTokenItem: RefreshToken.Item =
                        { Token = result.refresh_token
                          ClientId = clientId
                          UserId = item.UserId
                          ExpiresAt = DateTime.UtcNow.AddMinutes(float env.SSOCookieExpiresIn)
                          Scopes = item.RequestedScopes
                          IsPerimeterClient = isPerimeterClient }

                    let! successData = getSuccessData dataContext clientId item.UserId

                    let evt =
                        UserLogInTokenSuccessEvent(item.Code, refreshTokenItem, successData)

                    return (result, evt)
                | None -> return! raiseTask (unAuthorized "user is not found")
            }
