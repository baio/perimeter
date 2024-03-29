﻿namespace PRR.API.Tests

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open Microsoft.Extensions.DependencyInjection
open PRR.API.Auth.Infra
open PRR.API.Tenant.Infra
open PRR.API.Tests.Utils
open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Auth.LogIn.TokenAuthorizationCode
open PRR.Domain.Auth.SignUp
open System
open System.Security.Cryptography
open PRR.Domain.Tenant
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module LogInAuthorizationCode =

    let signUpData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }

    let mutable authCode: string = null

    let mutable testContext: UserTestContext option = None

    let mutable permissionId: int option = None

    let redirectUri = "http://localhost:4200"

    let overrideServices (services: IServiceCollection) =
        //

        let serv =
            services
            |> Seq.find (fun f -> f.ServiceType = typeof<IAuthStringsGetterProvider>)

        services.Remove(serv) |> ignore

        let authStringsGetter =
            (serv.ImplementationInstance :?> IAuthStringsGetterProvider)
                .AuthStringsGetter

        let _authStringsGetter: IAuthStringsGetter =
            { ClientId = authStringsGetter.ClientId
              ClientSecret = fun () -> "ClientSecret"
              AuthorizationCode = authStringsGetter.AuthorizationCode
              HS256SigningSecret = authStringsGetter.HS256SigningSecret
              RS256XMLParams = authStringsGetter.RS256XMLParams
              GetIssuerUri = authStringsGetter.GetIssuerUri
              GetAudienceUri = authStringsGetter.GetAudienceUri }


        services.AddSingleton<IAuthStringsGetterProvider>(AuthStringsProvider _authStringsGetter)
        |> ignore

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``login-authorization-code-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            task {
                testContext <-
                    Some(createUserTestContextWithServicesOverrides2 (fun _ -> ()) overrideServices testFixture)

                let! _ = createUser testContext.Value signUpData
                ()
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``B Login with wrong ClientSecret should give error``() =

            let clientId =
                testContext
                    .Value
                    .GetTenant()
                    .TenantManagementApplicationClientId

            let logInData: AuthorizeData =
                { Client_Id = clientId
                  Response_Type = "code"
                  State = "state"
                  Redirect_Uri = redirectUri
                  Scope = "openid profile email"
                  Email = signUpData.Email
                  Password = signUpData.Password
                  Code_Challenge = null
                  Code_Challenge_Method = null
                  Nonce = null
                  Prompt = None }


            task {
                let! code = logIn testFixture logInData

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenAuthorizationCode.Models.Data =
                    { Grant_Type = "authorization_code"
                      Code = code
                      Redirect_Uri = logInData.Redirect_Uri
                      Client_Id = clientId
                      Code_Verifier = null
                      Client_Secret = "xxx" }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }


        [<Fact>]
        [<Priority(3)>]
        member __.``C Login with correct data should success``() =

            let clientId =
                testContext
                    .Value
                    .GetTenant()
                    .TenantManagementApplicationClientId

            let logInData: AuthorizeData =
                { Client_Id = clientId
                  Response_Type = "code"
                  State = "state"
                  Redirect_Uri = redirectUri
                  Scope = "openid profile email offline_access"
                  Email = signUpData.Email
                  Password = signUpData.Password
                  Code_Challenge = null
                  Code_Challenge_Method = null
                  Nonce = null
                  Prompt = None }

            task {
                let! result = logIn testFixture logInData
                authCode <- result

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenAuthorizationCode.Models.Data =
                    { Grant_Type = "authorization_code"
                      Code = authCode
                      Redirect_Uri = logInData.Redirect_Uri
                      Client_Id = clientId
                      Code_Verifier = null
                      Client_Secret = "ClientSecret" }

                let! result' = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData

                do! ensureSuccessAsync result'

                let! result = result' |> readAsJsonAsync<LogInResult>

                result.access_token |> should be (not' Empty)
                result.id_token |> should be (not' Empty)
                result.refresh_token |> should be (not' Empty)
            }


        [<Fact>]
        [<Priority(4)>]
        member __.``D Login token with the same code should give 401``() =
            let loginTokenData: PRR.Domain.Auth.LogIn.TokenAuthorizationCode.Models.Data =
                { Grant_Type = "authorization_code"
                  Code = authCode
                  Redirect_Uri = redirectUri
                  Client_Id =
                      testContext
                          .Value
                          .GetTenant()
                          .TenantManagementApplicationClientId
                  Code_Verifier = null
                  Client_Secret = "ClientSecret" }

            task {
                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }
