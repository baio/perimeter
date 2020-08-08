namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Common.Domain.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Models
open PRR.System.Models
open System
open System.Security.Cryptography

[<AutoOpen>]
module SignIn =

    let validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "code" data.Code)
           (validateNullOrEmpty "grant_type" data.Grant_Type)
           (validateContains [| "code" |] "grant_ype" data.Grant_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNullOrEmpty "code_verifier" data.Code_Verifier) |]
        |> Array.choose id

    let logInToken: LogInToken =
        fun env item data ->
            let dataContext = env.DataContext
            if (item.ExpiresAt < DateTime.UtcNow) then raise (unAuthorized "code expires")
            if data.Client_Id <> item.ClientId then raise (unAuthorized "client_id mismatch")
            if data.Redirect_Uri <> item.RedirectUri then raise (unAuthorized "redirect_uri mismatch")
            let codeChallenge = env.Sha256Provider data.Code_Verifier
            if codeChallenge <> item.CodeChallenge then raise (unAuthorized "code_verifier code_challenge mismatch")
            task {
                match! getUserDataForToken dataContext item.UserId with
                | Some tokenData ->
                    let! result = signInUser env tokenData data.Client_Id
                    let evt = item.Code |> UserLogInTokenSuccessEvent
                    return (result, evt)
                | None ->
                    return! raiseTask (unAuthorized "User is not found")
            }
