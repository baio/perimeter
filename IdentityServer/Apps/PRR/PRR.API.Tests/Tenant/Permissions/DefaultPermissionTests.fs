namespace PRR.API.Tests.Tenant.Permissions

open Akkling
open Common.Test.Utils
open Common.Utils
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
open TaskUtils

module DefaultPermissions =

    let user1Data: Data =
        { FirstName = "First"
          LastName = "XXX"
          Email = "user1@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let user2Data: Data =
        { FirstName = "Second"
          LastName = "YYY"
          Email = "user2@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let private users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data
                 Token = None
                 Tenant = None
                 PermissionIds = Seq.empty<int> |}
              {| Data = user2Data
                 Token = None
                 Tenant = None
                 PermissionIds = Seq.empty<int> |} ]

    let mutable testContext: UserTestContext option = None

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          description: string
          dateCreated: System.DateTime }


    let permissionName1 = "test:permission:1"
    let permissionNameA = "test:permission:a"
    let permissionName2 = "test:permission:2"

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``permissions-default-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.BeforeAll() =
            task {
                let testPermission: PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      IsDefault = true }

                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant + permission
                let u = users.[0]

                let! userToken = createUser testContext.Value u.Data

                let tenant = testContext.Value.GetTenant()

                let! permissionId1 =
                    (testFixture.HttpPostAsync
                        userToken
                         (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                         { testPermission with
                               Name = permissionName1 })
                    >>= (readAsJsonAsync<int>)

                let! permissionId2 =
                    (testFixture.HttpPostAsync
                        userToken
                         (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                         { testPermission with
                               Name = permissionNameA })
                    >>= (readAsJsonAsync<int>)

                users.[0] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionIds =
                                        seq {
                                            permissionId1
                                            permissionId2
                                        } |}

                // create user 2 + tenant + permission
                let u = users.[1]

                let! userToken = createUser testContext.Value u.Data

                let tenant = testContext.Value.GetTenant()

                let! permissionId =
                    testFixture.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                        { testPermission with
                              Name = permissionName2 }
                    >>= (readAsJsonAsync<int>)

                users.[1] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionIds = seq { permissionId } |}
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``user 2 can access default scope of user's 1 domain``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {

                let! result =
                    logInUser'
                        [ permissionName1 ]
                        testFixture
                        u1.Tenant.Value.SampleApplicationClientId
                        u2.Data.Email
                        u2.Data.Password

                result.access_token |> should be (not' Empty)

                let jwtToken = ReadToken.readToken result.access_token

                jwtToken.IsSome |> should be True

                let scope =
                    jwtToken.Value.Claims
                    |> Seq.find (fun f -> f.Type = "scope")

                scope |> should be (not' Null)

                scope.Value
                |> String.split ' '
                |> should contain permissionName1

                scope.Value
                |> String.split ' '
                |> should not' (contain permissionNameA)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``when user 2 assigned some role in user's 1 domain it must be assigned requested scope explicitly``()
                                                                                                                      =
            let u1 = users.[0]
            let u2 = users.[1]
            task {

                // create new role for user's 1 domain
                let data: Roles.PostLike =
                    { Name = "role"
                      Description = "role description"
                      PermissionIds = [ u1.PermissionIds |> Seq.item 1 ] }

                let! result =
                    testFixture.HttpPostAsync
                        u1.Token.Value
                        (sprintf "/api/tenant/domains/%i/roles" u1.Tenant.Value.DomainId)
                        data

                do! ensureSuccessAsync result

                let! roleId = readAsJsonAsync<int> result

                // assign new role to user 2
                let data: DomainUserRoles.PostLike =
                    { UserEmail = u2.Data.Email
                      RolesIds = [ roleId ] }

                let! result =
                    testFixture.HttpPostAsync
                        u1.Token.Value
                        (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId)
                        data

                do! ensureSuccessAsync result

                //login with request permissionName1

                let! result =
                    logInUser'
                        [ permissionName1 ]
                        testFixture
                        u1.Tenant.Value.SampleApplicationClientId
                        u2.Data.Email
                        u2.Data.Password

                result.access_token |> should be (not' Empty)

                let jwtToken = ReadToken.readToken result.access_token

                jwtToken.IsSome |> should be True

                let scope =
                    jwtToken.Value.Claims
                    |> Seq.find (fun f -> f.Type = "scope")

                scope |> should be (not' Null)

                scope.Value
                |> String.split ' '
                |> should not' (contain permissionName1)
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``when user 2 assigned some role in user's 1 domain it should return requested scope for scopes in the role``()
                                                                                                                               =
            let u1 = users.[0]
            let u2 = users.[1]
            task {

                let! result =
                    logInUser'
                        [ permissionNameA ]
                        testFixture
                        u1.Tenant.Value.SampleApplicationClientId
                        u2.Data.Email
                        u2.Data.Password

                result.access_token |> should be (not' Empty)

                let jwtToken = ReadToken.readToken result.access_token

                jwtToken.IsSome |> should be True

                let scope =
                    jwtToken.Value.Claims
                    |> Seq.find (fun f -> f.Type = "scope")

                scope |> should be (not' Null)

                scope.Value
                |> String.split ' '
                |> should not' (contain permissionName1)

                scope.Value
                |> String.split ' '
                |> should contain permissionNameA
            }
