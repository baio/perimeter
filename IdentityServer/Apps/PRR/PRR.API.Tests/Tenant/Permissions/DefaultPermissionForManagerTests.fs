namespace PRR.API.Tests.Tenant.Permissions

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open FsUnit
open PRR.API.Tests.Utils
open PRR.Data.DataContext.Seed
open PRR.Data.Entities
open PRR.Domain.Auth.Common
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Permissions
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open DataAvail.Common.TaskUtils

// Management permissions shouldn't interfere with common permissions
// Owner is client's user scenario

module DefaultPermissionsForManager =

    let user: Data =
        { FirstName = "First"
          LastName = "XXX"
          Email = "user1@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }

    let mutable testContext: UserTestContext option = None
    let mutable tenant: CreatedTenantInfo option = None

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          description: string
          dateCreated: System.DateTime }

    let permissionName = "test:permission"

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``permissions-manager-default-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.BeforeAll() =
            task {
                let testPermission: PostLike =
                    { Name = permissionName
                      Description = "test description"
                      IsDefault = true }

                testContext <- Some(createUserTestContext testFixture)

                let! userToken = createUser testContext.Value user


                let tenant' = testContext.Value.GetTenant()

                let! permissionId1 =
                    (testFixture.Server2.HttpPostAsync
                        userToken
                         (sprintf "/api/tenant/apis/%i/permissions" tenant'.SampleApiId)
                         testPermission)
                    >>= (readAsJsonAsync<int>)

                tenant <- (Some tenant')

            }

        [<Fact>]
        [<Priority(1)>]
        member __.``user can access default scope``() =
            task {

                let! result =
                    logInUser'
                        [ permissionName ]
                        testFixture
                        tenant.Value.SampleApplicationClientId
                        user.Email
                        user.Password

                result.access_token |> should be (not' Empty)

                let jwtToken = readToken result.access_token

                jwtToken.IsSome |> should be True

                let scope =
                    jwtToken.Value.Claims
                    |> Seq.find (fun f -> f.Type = "scope")

                scope |> should be (not' Null)

                scope.Value
                |> String.split ' '
                |> should contain permissionName

            }
