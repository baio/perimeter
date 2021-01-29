namespace PRR.API.Routes

open PRR.Domain.Models
open DataAvail.Giraffe.Common
open DataAvail.KeyValueStorage.Core
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open MongoDB.Driver
open PRR.API.Configuration

open PRR.API.Routes.Auth
open PRR.Data.DataContext
open PRR.Domain.Auth.Common.KeyValueModels
open PRR.Domain.Auth.SignUpConfirm
open System
open System.Linq
open System.Threading
open System.Threading.Tasks
open DataAvail.EntityFramework.Common

[<CLIMutable>]
type ReinitData = { LoginAsDomain: bool }

module E2E =

    let private dropDatabase (connectionString: string) (dbName: string) =
        let client = MongoClient(connectionString)
        client.DropDatabase(dbName)

    let private recreateMongoDb (ctx: HttpContext) =
        let config = ctx.GetService<IConfiguration>()
        dropDatabase
            (config.GetValue "MongoKeyValueStorage:ConnectionString")
            (config.GetValue "MongoKeyValueStorage:DbName")
        dropDatabase
            (config.GetValue "MongoViewStorage:ConnectionString")
            (config.GetValue "MongoViewStorage:DbName")

    let private recreatedDataContextDb ctx =
        let dataContext = getDataContext ctx
        dataContext.Database.EnsureDeleted() |> ignore
        dataContext.Database.Migrate() |> ignore

    let private recreatedDbs ctx =
        recreatedDataContextDb ctx
        recreateMongoDb ctx

    type CreateUserTenantEnv =
        { DataContext: DbDataContext
          Config: AppConfig
          AuthStringsGetter: AuthStringsGetter }

    let createUserTenant (env: CreateUserTenantEnv) userId email =

        let config = env.Config

        let authConfig: PRR.Domain.Tenant.Models.AuthConfig =
            { AccessTokenSecret = config.Auth.Jwt.AccessTokenSecret
              AccessTokenExpiresIn = config.Auth.Jwt.AccessTokenExpiresIn
              IdTokenExpiresIn = config.Auth.Jwt.IdTokenExpiresIn
              RefreshTokenExpiresIn = config.Auth.Jwt.RefreshTokenExpiresIn }

        PRR.Domain.Tenant.CreateUserTenant.createUserTenant
            { DbDataContext = env.DataContext
              AuthConfig = authConfig
              AuthStringsGetter = env.AuthStringsGetter }

            { Email = email; UserId = userId }


    let createRoutes () =

        choose [ route "/e2e/reset"
                 >=> POST
                 >=> fun next ctx ->
                         // Recreate db on start
                         recreatedDbs ctx
                         Successful.NO_CONTENT next ctx

                 route "/e2e/reinit"
                 >=> POST
                 >=> fun next ctx ->
                         let dataContext = getDataContext ctx


                         recreatedDbs ctx
                         // signup
                         let signUpConfirmItem: SignUpKV =
                             { FirstName = "test"
                               LastName = "user"
                               Email = "hahijo5833@acceptmail.net"
                               Password = (getPasswordSalter ctx) "#6VvR&^"
                               Token = ""
                               ExpiredAt = DateTime.UtcNow.AddDays(1.)
                               QueryString = None }


                         let signUpEnv = PostSignUpConfirm.getEnv ctx

                         let kvStorage = getKeyValueStorage ctx

                         let kvStorage' =
                             { new IKeyValueStorage with
                                 member __.AddValue k (v: 'a) x = kvStorage.AddValue<'a> k v x
                                 member __.GetValue<'a> k x =
                                     (Task.FromResult(Result.Ok((box signUpConfirmItem) :?> 'a)))

                                 member __.RemoveValue<'a> k x = kvStorage.RemoveValue<'a> k x

                                 member __.RemoveValuesByTag<'a> k x = kvStorage.RemoveValuesByTag<'a> k x }



                         // login
                         let loginEnv: PRR.Domain.Auth.LogInToken.SignInUserEnv =
                             let config = getConfig ctx

                             { DataContext = getDataContext ctx
                               HashProvider = getHash ctx
                               JwtConfig = config.Auth.Jwt
                               Logger = getLogger ctx }

                         task {
                             let! userId =
                                 signUpConfirm
                                     { signUpEnv with
                                           KeyValueStorage = kvStorage' }
                                     { Token = "xxx" }
                             //
                             Thread.Sleep(100)

                             //
                             let env =
                                 { DataContext = getDataContext ctx
                                   AuthStringsGetter = getAuthStringsGetter ctx
                                   Config = getConfig ctx }

                             let! _ = createUserTenant env userId signUpConfirmItem.Email

                             let! data = ctx |> bindJsonAsync<ReinitData>

                             let! clientId =
                                 match data.LoginAsDomain with
                                 | true ->
                                     query {
                                         for dur in dataContext.DomainUserRole do
                                             where
                                                 (dur.RoleId = PRR.Data.DataContext.Seed.Roles.DomainOwner.Id
                                                  && dur.UserEmail = signUpConfirmItem.Email)

                                             select
                                                 (dur.Domain.Applications.Single(fun p -> p.IsDomainManagement).ClientId)
                                     }
                                     |> toSingleAsync
                                 | _ -> Task.FromResult "__DEFAULT_CLIENT_ID__"


                             let! res =
                                 PRR.Domain.Auth.LogInEmail.logInEmail
                                     loginEnv
                                     clientId
                                     signUpConfirmItem.Email
                                     signUpConfirmItem.Password

                             let (result, _, _) = res

                             ctx.Response.Cookies.Delete("sso")

                             return! Successful.OK result next ctx
                         } ]
