namespace PRR.API.Tests

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

module LogInPassword =

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
              RS256XMLParams = authStringsGetter.RS256XMLParams }


        services.AddSingleton<IAuthStringsGetterProvider>(AuthStringsProvider _authStringsGetter)
        |> ignore

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``login-password-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            task {
                testContext <- Some(createUserTestContext testFixture)

                let! _ = createUser testContext.Value signUpData
                ()
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``B Login with wrong ClientSecret should give error``() =

            task {

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword.Models.Data =
                    { Grant_Type = "password"
                      Client_Id = "xxxx"
                      Username = signUpData.Email
                      Password = signUpData.Password
                      Scope = "openid profile email" }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``B.2 Login with wrong email should give error``() =

            let clientId =
                testContext
                    .Value
                    .GetTenant()
                    .TenantManagementApplicationClientId

            task {

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword.Models.Data =
                    { Grant_Type = "password"
                      Client_Id = clientId
                      Username = "email@email.com"
                      Password = signUpData.Password
                      Scope = "openid profile email" }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B.3 Login with wrong password should give error``() =

            let clientId =
                testContext
                    .Value
                    .GetTenant()
                    .TenantManagementApplicationClientId

            task {

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword.Models.Data =
                    { Grant_Type = "password"
                      Client_Id = clientId
                      Username = signUpData.Email
                      Password = "1111"
                      Scope = "openid profile email" }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Login with correct data should success``() =

            task {

                let clientId =
                    testContext
                        .Value
                        .GetTenant()
                        .TenantManagementApplicationClientId

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword.Models.Data =
                    { Grant_Type = "password"
                      Client_Id = clientId
                      Username = signUpData.Email
                      Password = signUpData.Password
                      Scope = "openid profile email offline_access" }

                let! result' = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do! ensureSuccessAsync result'

                let! result = result' |> readAsJsonAsync<LogInResult>

                result.access_token |> should be (not' Empty)
                result.id_token |> should be (not' Empty)
                result.refresh_token |> should be (not' Empty)
            }
