namespace PRR.API.Tests.Auth

open System.Text.RegularExpressions
open Common.Domain.Models
open FSharpx
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open Microsoft.Extensions.DependencyInjection
open PRR.API.Infra

module SocialTests =

    let mutable state: string = null

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "test123"
          QueryString = null }

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let socialClientId = "xxx"

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``social-test-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(0)>]
        member __.``0 BEFORE ALL``() =

            task {
                testContext <- Some(createUserTestContext testFixture)
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
                    testFixture.HttpPostAsync userToken (sprintf "/api/tenant/domains/%i/social/github" domainId) data

                ()
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``1 Redirect to signin with github``() =
            task {
                let tenant = testContext.Value.GetTenant()
                let clientId = tenant.SampleApplicationClientId

                let url = "/api/auth/social"

                let data: PRR.Domain.Auth.Social.Models.Data =
                    { Social_Name = "github"
                      Client_Id = clientId
                      Response_Type = "code"
                      State = "state"
                      Redirect_Uri = "http://localhost:4200"
                      Scope = "openid profile email"
                      Code_Challenge = "123"
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

                let! result = testFixture.HttpPostFormAsync' url data
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
        member __.``2 Github callback``() =

            let httpRequestFun: HttpRequestFun = fun req -> ()

            testFixture.OverrideServices(fun services ->
                let sp = services.BuildServiceProvider()
                services.AddSingleton<IHttpRequestFunProvider>(HttpRequestFunProvider httpRequestFun)
                |> ignore)

            task {
                let tenant = testContext.Value.GetTenant()

                let url =
                    sprintf "/api/auth/social/callback?code=111&state=%s" state

                let! result = testFixture.HttpGetAsync' url
                do! ensureRedirectSuccessAsync result

                ()
            }
