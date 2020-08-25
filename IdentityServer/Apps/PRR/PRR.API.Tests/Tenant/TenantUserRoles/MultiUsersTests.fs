﻿namespace PRR.API.Tests.Tenant.TenantUserRoles

open Akkling
open Common.Test.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open FsUnit
open PRR.API.Routes.Tenant
open PRR.API.Tests.Utils
open PRR.Data.Entities
open PRR.Domain.Auth
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.DomainUserRoles
open PRR.System.Models
open TaskUtils
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module MultiUsers =

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

    let newUserEmail = "new@user.com"

    let private users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data
                 Token = None
                 Tenant = None |}
              {| Data = user2Data
                 Token = None
                 Tenant = None |} ]


    let mutable testContext: UserTestContext option = None




    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``tenant-user-roles-multi-users-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {

                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant
                let u1 = users.[0]
                let! userToken = createUser testContext.Value u1.Data
                let tenant = testContext.Value.GetTenant()

                users.[0] <- {| u1 with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant) |}

                // add user 2 role as first tenant sample domain admin
                let data: PostLike =
                    { UserEmail = users.[1].Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }

                let u1 = users.[0]

                let! _ = testFixture.HttpPostAsync u1.Token.Value
                             (sprintf "/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data

                // create user 2
                let u2 = users.[1]
                let! _ = createUser testContext.Value u2.Data
                
                let! res = logInUser testFixture u1.Tenant.Value.SampleApplicationClientId u2.Data.Email u2.Data.Password

                users.[1] <- {| u2 with
                        Token = Some(res.AccessToken)
                        Tenant = Some(tenant) |}
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Same tenant user but with no MANAGE_TENANT_DOMAINS permission forbidden create domain pool``() =
            let u2 = users.[1]
            task {
                let data: DomainPools.PostLike =
                    { Name = "Domain pool 2" }
                let! result = testFixture.HttpPutAsync u2.Token.Value
                                  (sprintf "/tenant/domain-pools/%i" u2.Tenant.Value.DomainPoolId) data
                ensureForbidden result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Add to user 2 permission to create domains in tenant 1``() =
            // re-signin user 1 under tenant management domain
            let u1 = users.[0]

            task {
                let! res = logInUser testFixture u1.Tenant.Value.TenantManagementApplicationClientId u1.Data.Email u1.Data.Password
                
                users.[0] <- {| u1 with Token = Some(res.AccessToken) |}

                // add user 2 manage_domains role under tenant 1
                let data: PostLike =
                    { UserEmail = users.[1].Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.TenantAdmin.Id ] }

                let u1 = users.[0]
                
                let token = u1.Token.Value               

                let! res = testFixture.HttpPostAsync token  "/tenant/admins" data

                do! ensureSuccessAsync res
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C User 2 can update domain pools under tenant 1``() =
            // re-signin user 2 under tenant 1 management domain
            let u1 = users.[0]
            let u2 = users.[1]
            

            task {
                
                let! res = logInUser testFixture u1.Tenant.Value.TenantManagementApplicationClientId u2.Data.Email u2.Data.Password
           
                users.[1] <- {| u2 with Token = Some(res.AccessToken) |}
                let u2 = users.[1]
                //
                let data: DomainPools.PostLike =
                    { Name = "Domain pool 2" }
                let! result = testFixture.HttpPutAsync u2.Token.Value
                                  (sprintf "/tenant/domain-pools/%i" u1.Tenant.Value.DomainPoolId) data
                do! ensureSuccessAsync result

                ()
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D User 2 still can't add new admins under 1st tenant management domain``() =
            let data: PostLike =
                { UserEmail = "test123@mail.com"
                  RolesIds = [ PRR.Data.DataContext.Seed.Roles.TenantAdmin.Id ] }

            task {

                let! res = testFixture.HttpPostAsync users.[1].Token.Value "/tenant/admins" data

                ensureForbidden res
            }

        [<Fact>]
        [<Priority(6)>]
        member __.``E remove tenant owner should give error``() =
            let u1 = users.[0]
            task {
                let! result = testFixture.HttpDeleteAsync u1.Token.Value
                                  (sprintf "/tenant/admins/%s" u1.Data.Email)
                do ensureForbidden result }

        [<Fact>]
        [<Priority(7)>]
        member __.``F update tenant owner should give error``() =
            let u1 = users.[0]
            task {
                let data: PostLike =
                    { UserEmail = u1.Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }
                let! result = testFixture.HttpPostAsync u1.Token.Value
                                  (sprintf "/tenant/admins") data
                do ensureForbidden result
            }
