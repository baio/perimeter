namespace PRR.API.Tests.Auth

open System.Text.RegularExpressions
open System.Threading.Tasks
open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Models
open DataAvail.HttpRequest.Core
open FSharpx
open Newtonsoft.Json
open PRR.API.Tests.Utils
open PRR.Domain.Auth.LogIn.TokenAuthorizationCode
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open Microsoft.Extensions.DependencyInjection
open PRR.API.Auth.Infra
open DataAvail.Common
open System.Security.Cryptography

module SocialTests =

    let random = System.Random()

    let randomString length =

        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~"

        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat


    let codeVerifier = randomString 128

    let sha256 = SHA256.Create()

    let codeChallenge =
        SHA256.getSha256Base64Hash sha256 codeVerifier
        |> cleanupCodeChallenge


    let mutable state: string = null

    let userData: PRR.Domain.Auth.SignUp.Models.Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "test123"
          ReturnUrl = null }

    let mutable userToken: string = null

    let mutable authCode: string = null

    let mutable testContext: UserTestContext option = None

    let socialClientId = "xxx"

    // Services could be overriden once on test fixture start
    let overrideServices (services: IServiceCollection) =

        // override httpRequestFun to use for http requests (provides mock results)
        let httpRequestFun: HttpRequestFun =
            fun req ->
                match req.Uri with
                | Regex @"https:\/\/github\.com\/login\/oauth\/access_token" _ ->
                    ({| access_token = "xxx" |})
                    |> JsonConvert.SerializeObject
                    |> Task.FromResult
                | Regex @"https:\/\/api\.github\.com\/user" _ ->
                    ({| id = 100
                        avatar_url = "http://avatar"
                        email = "max@gmail.com"
                        name = "max p" |})
                    |> JsonConvert.SerializeObject
                    |> Task.FromResult
                | _ -> raise (exn "unknown request")

        let serv =
            services
            |> Seq.find (fun f -> f.ServiceType = typeof<IHttpRequestFunProvider>)

        services.Remove(serv) |> ignore

        services.AddSingleton<IHttpRequestFunProvider>(HttpRequestFunProvider httpRequestFun)
        |> ignore

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``social-test-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(0)>]
        member __.``* BEFORE ALL``() =

            task {
                testContext <- Some(createUserTestContextWithServicesOverrides overrideServices testFixture)

                let! userToken' = createUser' true testContext.Value userData
                userToken <- userToken'
                // create github social connection
                let data: SocialConnections.PostLike =
                    { ClientId = socialClientId
                      ClientSecret = "yyy"
                      Attributes = [| "aaa" |]
                      Permissions = [| "bbb" |] }

                let domainId = testContext.Value.GetTenant().DomainId

                let! _ =
                    testFixture.Server2.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/social/github" domainId)
                        data

                ()
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Redirect to signin with github``() =
            task {
                let tenant = testContext.Value.GetTenant()
                let clientId = tenant.SampleApplicationClientId

                let url = "/api/auth/social"

                let data: PRR.Domain.Auth.Social.SocialAuth.Models.Data =
                    { Social_Name = "github"
                      Client_Id = clientId
                      Response_Type = "code"
                      State = "state"
                      Redirect_Uri = "http://localhost:4200"
                      Scope = "openid profile email"
                      Code_Challenge = codeChallenge
                      Code_Challenge_Method = "S256" }

                let data =
                    (Map
                        (seq {
                            ("Social_Name", data.Social_Name)
                            ("Client_Id", data.Client_Id)
                            ("Response_Type", data.Response_Type)
                            ("State", data.State)
                            ("Redirect_Uri", data.Redirect_Uri)
                            ("Scope", data.Scope)
                            ("Code_Challenge", data.Code_Challenge)
                            ("Code_Challenge_Method", data.Code_Challenge_Method)
                         }))

                let! result = testFixture.Server1.HttpPostFormAsync' url data
                do! ensureRedirectSuccessAsync result
                let location = result.Headers.Location.ToString()

                let expectedUrl =
                    socialClientId
                    |> sprintf "https:\/\/github\.com\/login\/oauth\/authorize\?client_id=%s"
                    |> sprintf "%s&redirect_uri=(.+)&state=(\w+)"

                let expectedLocation = Regex expectedUrl

                (expectedLocation.IsMatch location)
                |> should equal true

                let matches = expectedLocation.Match(location)

                state <- matches.Groups.[2].Value

                ()
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``B Github callback``() =

            task {

                let url =
                    sprintf "/api/auth/social/callback?code=111&state=%s" state

                let! result = testFixture.Server1.HttpGetAsync' url

                do! ensureRedirectSuccessAsync result

                let location = result.Headers.Location.ToString()

                location |> should not' Empty

                match location with
                | Regex "code=(\w+)" [ code ] -> authCode <- code
                | x -> x |> should be Empty

                ()
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Login with correct data should success``() =

            let tenant = testContext.Value.GetTenant()
            let clientId = tenant.SampleApplicationClientId

            task {

                let loginTokenData: PRR.Domain.Auth.LogIn.TokenAuthorizationCode.Models.Data =
                    { Grant_Type = "authorization_code"
                      Code = authCode
                      Redirect_Uri = "http://localhost:4200"
                      Client_Id = clientId
                      Code_Verifier = codeVerifier
                      Client_Secret = null }

                let! result' = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData

                do! ensureSuccessAsync result'

                let! result =
                    result'
                    |> readAsJsonAsync<LogInResult>

                result.access_token |> should be (not' Empty)

                result.id_token |> should be (not' Empty)

                result.refresh_token |> should be (not' Empty)

                printfn "%s" result.access_token
            }
