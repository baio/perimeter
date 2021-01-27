namespace PRR.API.Tests.Tenant.SocialConnections
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.SocialConnections
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module CRUD =

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let userPassword = "123"

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable domainPoolId: int option = None

    let mutable domainId: int option = None

    [<CLIMutable>]
    type GetLikeDomainDto =
        { id: int
          name: string
          dateCreated: System.DateTime }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          dateCreated: System.DateTime
          domains: GetLikeDomainDto seq }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``social-connections-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser' true testContext.Value userData
                userToken <- userToken'
                domainId <- Some(testContext.Value.GetTenant().DomainId)
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create social connection must be success``() =
            task {
                let data: PostLike =
                    { ClientId = "xxx"
                      ClientSecret = "yyy"
                      Attributes = [| "aaa" |]
                      Permissions = [| "bbb" |] }

                let! result =
                    testFixture.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/social/github" domainId.Value)
                        data

                do! ensureSuccessAsync result

                ()
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Update social connection must be success``() =
            task {
                let data: PostLike =
                    { ClientId = "zzz1"
                      ClientSecret = "yyy"
                      Attributes = [| "aaa"; "ggg" |]
                      Permissions = [||] }

                let! result =
                    testFixture.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/social/github" domainId.Value)
                        data

                do! ensureSuccessAsync result

                ()
            }


        [<Fact>]
        [<Priority(3)>]
        member __.``C Get social connections must be success``() =
            task {

                let! result =
                    testFixture.HttpGetAsync userToken (sprintf "/api/tenant/domains/%i/social" domainId.Value)

                do! ensureSuccessAsync result

                let! actual = readAsJsonAsync<GetLike array> result

                let expected: GetLike array =
                    [| { Name = "github"
                         ClientId = "zzz1"
                         ClientSecret = "yyy"
                         Attributes = [| "aaa"; "ggg" |]
                         Permissions = [||]
                         Order = 0 } |]

                actual |> should equal expected

                ()
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D Reorder social connection must be success``() =
            task {
                let data: PostLike =
                    { ClientId = "xxx1"
                      ClientSecret = "yyy"
                      Attributes = [| "aaa" |]
                      Permissions = [| "bbb" |] }

                let! result =
                    testFixture.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/social/google" domainId.Value)
                        data

                do! ensureSuccessAsync result

                let data = System.Collections.Generic.Dictionary()

                data.Add("google", 1)

                data.Add("github", 0)

                let! result =
                    testFixture.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/social/order" domainId.Value)
                        data

                do! ensureSuccessAsync result
            }


        [<Fact>]
        [<Priority(5)>]
        member __.``E Delete social connections must be success``() =
            task {

                let! result =
                    testFixture.HttpDeleteAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/social/github" domainId.Value)

                do! ensureSuccessAsync result

                ()
            }
