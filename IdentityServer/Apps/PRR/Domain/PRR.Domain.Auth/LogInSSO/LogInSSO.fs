namespace PRR.Domain.Auth.LogInSSO

open Common.Domain.Models

open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.System.Models
open System
open PRR.Domain.Auth.ValidateScopes

[<AutoOpen>]
module Authorize =

    let validateData (data: Data): BadRequestError array =
        let scope =
            if data.Scope = null then "" else data.Scope

        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "response_type" data.Response_Type)
           (validateContains [| "code" |] "response_type" data.Response_Type)
           (validateNullOrEmpty "redirect_uri" data.Redirect_Uri)
           (validateUrl "redirect_uri" data.Redirect_Uri)
           (validateNullOrEmpty "scope" scope)
           (validateContainsAll [| "openid"; "profile" |] "scope" (scope.Split " "))
           (validateNullOrEmpty "code_challenge" data.Code_Challenge)
           (validateNullOrEmpty "code_challenge_method" data.Code_Challenge_Method)
           (validateContains [| "S256" |] "code_challenge_method" data.Code_Challenge_Method) |]
        |> Array.choose id

    let logInSSO: LogInSSO =
        fun env data sso ->

            if sso.ExpiresAt < DateTime.UtcNow then raise (unAuthorized "sso expired")

            let dataContext = env.DataContext
            task {

                let! (clientId, issuer) = PRR.Domain.Auth.LogIn.UserHelpers.getClientIdAndIssuer dataContext data.Client_Id sso.Email

                let! app =
                    query {
                        for app in dataContext.Applications do
                            where (app.ClientId = clientId)
                            select
                                (Tuple.Create(app.Domain.Pool.TenantId, app.Domain.TenantId, app.AllowedCallbackUrls))
                    }
                    |> toSingleExnAsync (unAuthorized ("client_id not found"))

                let (poolTenantId, managementDomainTenantId, callbackUrls) = app

                let tenantId =
                    if managementDomainTenantId.HasValue then managementDomainTenantId.Value else poolTenantId

                if tenantId <> sso.TenantId
                then return raise (unAuthorized "sso wrong tenant")

                if (callbackUrls
                    <> "*"
                    && (callbackUrls.Split(",")
                        |> Seq.map (fun x -> x.Trim())
                        |> Seq.contains data.Redirect_Uri
                        |> not)) then
                    return! raise (unAuthorized "return_uri mismatch")

                match! getUserId dataContext sso.Email with
                | Some userId ->
                    let code = env.CodeGenerator()

                    let result: PRR.Domain.Auth.LogIn.Models.Result =
                        { RedirectUri = data.Redirect_Uri
                          State = data.State
                          Code = code }

                    let expiresAt =
                        DateTime.UtcNow.AddMinutes(float env.CodeExpiresIn)

                    let scopes = data.Scope.Split " "

                    let! validatedScopes = validateScopes dataContext sso.Email clientId scopes

                    let loginItem: LogIn.Item =
                        { Code = code
                          ClientId = data.Client_Id
                          Issuer = issuer
                          CodeChallenge = data.Code_Challenge
                          RequestedScopes = scopes
                          ValidatedScopes = validatedScopes
                          UserId = userId
                          ExpiresAt = expiresAt
                          RedirectUri = data.Redirect_Uri }

                    let evt = UserLogInSuccessEvent(loginItem, None)

                    return (result, evt)

                | None -> return! raise (unAuthorized "Wrong email or password")
            }
