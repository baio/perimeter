namespace PRR.API.Tests.Utils

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Microsoft.Extensions.Configuration
open PRR.API.Auth.Configuration.ConfigureServices
open PRR.API.Auth.Infra
open PRR.API.Tenant.Infra
open PRR.Data.DataContext
open PRR.Domain.Auth
open System
open System.Security.Cryptography
open System.Web
open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Tenant
open PRR.Domain.Auth.LogIn.TokenAuthorizationCode

[<AutoOpen>]
module CreateUser =

    let logIn' (testFixture: TestFixture) (data: PRR.Domain.Auth.LogIn.Authorize.Models.Data) =
        testFixture.Server1.HttpPostFormAsync'
            "/api/auth/authorize"
            (Map
                (seq {
                    ("Client_Id", data.Client_Id)
                    ("Response_Type", data.Response_Type)
                    ("State", data.State)
                    ("Redirect_Uri", data.Redirect_Uri)
                    ("Scope", data.Scope)
                    ("Email", data.Email)
                    ("Password", data.Password)
                    ("Code_Challenge", data.Code_Challenge)
                    ("Code_Challenge_Method", data.Code_Challenge_Method)
                 }))

    let logIn (testFixture: TestFixture) (data: PRR.Domain.Auth.LogIn.Authorize.Models.Data) =
        task {
            let! result = logIn' testFixture data
            let location = readResponseHader "Location" result
            let uri = Uri(location)

            return
                HttpUtility
                    .ParseQueryString(uri.Query)
                    .Get("code")
        }

    let sha256 = SHA256.Create()
    let random = Random()



    [<CLIMutable>]
    type SignInResult =
        { idToken: string
          accessToken: string
          refreshToken: string }

    let private randomString length =

        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~"

        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat

    let private createCodeChallenge () =

        let codeVerifier = randomString 128

        (codeVerifier,
         (SHA256.getSha256Base64Hash sha256 codeVerifier)
         |> cleanupCodeChallenge)

    let logInUser' scopes (fixture: TestFixture) (clientId: string) (email: string) (password: string) =
        task {
            let redirectUri = "http://localhost:4200"

            let (codeVerifier, codeChallenge) = createCodeChallenge ()

            let clientId = clientId

            let scope =
                [ "openid"; "profile"; "email" ]
                |> Seq.append scopes
                |> String.concat " "

            let logInData: PRR.Domain.Auth.LogIn.Authorize.Models.Data =
                { Client_Id = clientId
                  Response_Type = "code"
                  State = "state"
                  Redirect_Uri = redirectUri
                  Scope = scope
                  Email = email
                  Password = password
                  Code_Challenge = codeChallenge
                  Code_Challenge_Method = "S256" }

            let! code = logIn fixture logInData

            let loginTokenData: Models.Data =
                { Grant_Type = "authorization_code"
                  Code = code
                  Redirect_Uri = logInData.Redirect_Uri
                  Client_Id = clientId
                  Code_Verifier = codeVerifier
                  Client_Secret = null }

            let! result = fixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData

            let! result = result |> readAsJsonAsync<LogInResult>

            return result
        }

    let logInUser = logInUser' Seq.empty

    let createUser'' signInUnderSampleDomain (env: UserTestContext) (userData: SignUp.Models.Data) =
        task {
            let! _ = env.TestFixture.Server1.HttpPostAsync' "/api/auth/sign-up" userData
            env.ConfirmTokenWaitHandle.WaitOne() |> ignore
            let confirmData: SignUpConfirm.Models.Data = { Token = env.GetConfirmToken() }

            let! result = env.TestFixture.Server1.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

            let! userId = readAsJsonAsync<int> result


            //
            let services = env.TestFixture.Server1.Server.Services

            let configProvider =
                services.GetService(typeof<IConfigProvider>) :?> IConfigProvider

            let dataContext =
                services.GetService(typeof<DbDataContext>) :?> DbDataContext

            let config =
                services.GetService(typeof<IConfiguration>) :?> IConfiguration

            let authStringsGetterProvider =
                env.TestFixture.Server2.Server.Services.GetService(typeof<IAuthStringsGetterProvider>) :?> IAuthStringsGetterProvider

            let config' =
                PRR.API.Tenant.Configuration.CreateAppConfig.createAppConfig (config)

            let env': CreateUserTenant.Env =
                { DbDataContext = dataContext
                  AuthStringsGetter = authStringsGetterProvider.AuthStringsGetter
                  AuthConfig = config'.TenantAuth }

            let! tenant =
                createUserTenant
                    env'
                    { UserId = userId
                      Email = userData.Email }

            env.SetTenant tenant

            let clientId =
                if signInUnderSampleDomain
                then tenant.DomainManagementApplicationClientId
                else tenant.TenantManagementApplicationClientId


            let! result = logInUser env.TestFixture clientId userData.Email userData.Password

            return result
        }

    let createUser' signInUnderSampleDomain b c =
        task {
            let! result = createUser'' signInUnderSampleDomain b c
            return result.access_token
        }

    let createUser = createUser' true

    let createUserNoTenant (env: UserTestContext) (userData: SignUp.Models.Data) =
        task {
            let! _ = env.TestFixture.Server1.HttpPostAsync' "/api/auth/sign-up" userData
            env.ConfirmTokenWaitHandle.WaitOne() |> ignore
            let confirmData: SignUpConfirm.Models.Data = { Token = env.GetConfirmToken() }

            let! _ = env.TestFixture.Server1.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

            let clientId = "__DEFAULT_CLIENT_ID__"

            let! result = logInUser env.TestFixture clientId userData.Email userData.Password

            return result.access_token
        }
