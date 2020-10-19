namespace PRR.API.Tests.Auth

open System.Text.RegularExpressions
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

module SocialTests =

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
        member __.``1 Signin with github``() =
            task {
                let tenant = testContext.Value.GetTenant()
                let clientId = tenant.SampleApplicationClientId

                let url =
                    sprintf "/api/auth/social?clientId=%s&socialName=github" clientId

                let! result = testFixture.HttpGetAsync' url
                do! ensureRedirectSuccessAsync result
                let location = result.Headers.Location.ToString()

                let expectedUrl =
                    socialClientId
                    |> sprintf "https:\/\/github\.com\/login\/oauth\/authorize\?client_id=%s"
                    |> sprintf "%s&redirect_uri=(\w+)&state=(\w+)"                                        

                let expectedLocation = Regex expectedUrl

                //(expectedLocation.IsMatch location) |> should equal true

                ()
            }
