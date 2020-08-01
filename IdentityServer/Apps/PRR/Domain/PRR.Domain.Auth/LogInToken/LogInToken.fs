namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Common.Domain.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Models
open PRR.System.Models

[<AutoOpen>]
module SignIn =

    let validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.ClientId)
           (validateNullOrEmpty "response_type" data.GrantType)
           (validateContains [| "code" |] "response_type" data.GrantType)
           (validateNullOrEmpty "redirect_uri" data.RedirectUri)
           (validateUrl "redirect_uri" data.RedirectUri)
           (validateNullOrEmpty "email" data.CodeVerifier) |]
        |> Array.choose id

    let logInToken: LogInToken =
        fun env item data ->
            let dataContext = env.DataContext
            task {
                match! getUserDataForToken dataContext item.UserId with
                | Some tokenData ->
                    let! result = signInUser env tokenData data.ClientId
                    let evt = item.Code |> UserLogInTokenSuccessEvent
                    return (result, evt)
                | None ->
                    return! raiseTask UnAuthorized
            }
