namespace PRR.API.Tests

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.DependencyInjection
open PRR.API.Tenant.Infra
open PRR.API.Tests.Utils
open PRR.Domain.Tenant
open PRR.Domain.Auth.SignUp
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module LogInClientCredentials =

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
    type ``login-client-credentials-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            task {
                testContext <- Some(createUserTestContextWithServicesOverrides2 (fun _ -> ()) overrideServices testFixture)
                let! _ = createUser testContext.Value signUpData
                ()
            }


        [<Fact>]
        [<Priority(1)>]
        member __.``A.1 Login with wrong ClientSecret should give error``() =

            task {

                let tenant = testContext.Value.GetTenant()

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenClientCredentials.Models.Data =
                    { Grant_Type = "client_credentials"
                      Client_Id = tenant.DomainManagementApplicationClientId
                      Client_Secret = "zzz"
                      Audience = tenant.DomainManagementApiIdentifier }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }


        [<Fact>]
        [<Priority(1)>]
        member __.``A.2 Login with wrong clientId should give error``() =

            let tenant = testContext.Value.GetTenant()

            task {

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenClientCredentials.Models.Data =
                    { Grant_Type = "client_credentials"
                      Client_Id = "xxxx"
                      Client_Secret = "ClientSecret"
                      Audience = tenant.DomainManagementApiIdentifier }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(1)>]        
        member __.``A.3 Login with wrong Audience should give error``() =

            let tenant = testContext.Value.GetTenant()

            task {

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenClientCredentials.Models.Data =
                    { Grant_Type = "client_credentials"
                      Client_Id = tenant.DomainManagementApplicationClientId
                      Client_Secret = "ClientSecret"
                      Audience = "xxxxx" }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Login with correct data should success``() =

            let tenant = testContext.Value.GetTenant()

            task {

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenClientCredentials.Models.Data =
                    { Grant_Type = "client_credentials"
                      Client_Id = tenant.DomainManagementApplicationClientId
                      Client_Secret = "ClientSecret"
                      Audience = tenant.DomainManagementApiIdentifier }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do! ensureSuccessAsync result
            }
