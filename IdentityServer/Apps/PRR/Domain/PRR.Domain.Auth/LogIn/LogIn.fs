namespace PRR.Domain.Auth.LogIn

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
           (validateNullOrEmpty "email" data.Email)
           (validateEmail "email" data.Email)
           (validateNullOrEmpty "password" data.Password)
           (validateNullOrEmpty "code_challenge" data.Code_Challenge)
           (validateNullOrEmpty "code_challenge_method" data.Code_Challenge_Method)
           (validateContains [| "S256" |] "code_challenge_method" data.Code_Challenge_Method) |]
        |> Array.choose id

    let private getClientAppData' (dataContext: DbDataContext) clientId =
        query {
            for app in dataContext.Applications do
                where (app.ClientId = clientId)
                select
                    (Tuple.Create
                        (app.SSOEnabled,
                         app.AllowedCallbackUrls,
                         Nullable(app.Domain.Pool.TenantId),
                         app.Domain.TenantId))
        }
        |> toSingleExnAsync (unAuthorized ("client_id not found"))

    let private getClientAppData (dataContext: DbDataContext) clientId =
        task {
            if clientId = PRR.Domain.Auth.Constants.PERIMETER_CLIENT_ID
            then return (true, "*", Nullable(), Nullable())
            else return! getClientAppData' dataContext clientId
        }


    type LoginData =
        { UserId: int
          ClientId: ClientId
          ResponseType: string
          State: string
          RedirectUri: string
          Scope: Scope
          Email: string
          CodeChallenge: string
          CodeChallengeMethod: string }

    let logInUser env sso (data: LoginData) =
        let dataContext = env.DataContext
        task {

            printfn "login:1 %s %s" data.ClientId data.Email
            let! { ClientId = clientId; Issuer = issuer } =
                getAppInfo env.DataContext data.ClientId data.Email 1<minutes>

            printfn "login:2 %s %s" clientId issuer
            let! (ssoEnabled, callbackUrls, poolTenantId, domainTenantId) = getClientAppData dataContext clientId
            printfn "login:3"

            let tenantId =
                match (poolTenantId.HasValue, domainTenantId.HasValue) with
                // Perimeter app : profile / create tenant
                | (false, false) -> PRR.Domain.Auth.Constants.PERIMETER_TENANT_ID
                // Domain app
                | (true, false) -> poolTenantId.Value
                // Tenant management app
                | (false, true) -> domainTenantId.Value
                | (true, true) -> raise (unexpected "Both poolTenantId, domainTenantId defined")

            if (callbackUrls
                <> "*"
                && (callbackUrls.Split(",")
                    |> Seq.map (fun x -> x.Trim())
                    |> Seq.contains data.RedirectUri
                    |> not)) then
                return! raise (unAuthorized "return_uri mismatch")


            let code = env.CodeGenerator()

            let result: Result =
                { RedirectUri = data.RedirectUri
                  State = data.State
                  Code = code }

            let codeExpiresAt =
                DateTime.UtcNow.AddMinutes(float env.CodeExpiresIn)

            let scopes = data.Scope.Split " "

            let! validatedScopes = validateScopes dataContext data.Email clientId scopes

            printfn "login:4 %s %A %A" clientId scopes validatedScopes

            let userId = data.UserId

            let loginItem: LogIn.Item =
                { Code = code
                  ClientId = data.ClientId
                  Issuer = issuer
                  CodeChallenge = data.CodeChallenge
                  RequestedScopes = scopes
                  ValidatedScopes = validatedScopes
                  UserId = userId
                  ExpiresAt = codeExpiresAt
                  RedirectUri = data.RedirectUri }

            let ssoExpiresAt =
                DateTime.UtcNow.AddMinutes(float env.SSOExpiresIn)

            let ssoItem =
                match ssoEnabled, sso with
                | (true, Some sso) ->
                    Some
                        ({ Code = sso
                           UserId = userId
                           // Used only in SSO login, user without tenant should not be able to sso login somewhere anyway
                           TenantId = tenantId
                           ExpiresAt = ssoExpiresAt
                           Email = data.Email }: SSO.Item)
                // TODO : Handle case SSO enabled but sso token not found
                | _ -> None

            let evt =
                UserLogInSuccessEvent(loginItem, ssoItem)

            return (result, evt)
        }


    let logIn: LogIn =
        fun sso env data ->
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

                    return! logInUser env sso loginData

                | None -> return! raise (unAuthorized "Wrong email or password")
            }
