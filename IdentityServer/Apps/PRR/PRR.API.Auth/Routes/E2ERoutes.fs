namespace PRR.API.Auth.Routes

#if E2E

open DataAvail.Giraffe.Common
open DataAvail.Http.Exceptions
open DataAvail.KeyValueStorage.Core
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open MongoDB.Driver
open PRR.API.Auth.Configuration

open PRR.API.Auth.Routes
open PRR.Domain.Auth.Common.KeyValueModels
open PRR.Domain.Auth.SignUpConfirm
open System
open System.Linq
open System.Threading
open System.Threading.Tasks
open DataAvail.EntityFramework.Common
open Microsoft.Extensions.Logging
open PRR.Domain.Models

[<CLIMutable>]
type ReinitData = { LoginAsDomain: bool }

module E2E =

    // TODO : Either separate E2E.API project either create tenants from client tests directly !!!
    open System.Security.Cryptography
    let private random = Random()

    let private getRandomString length =
        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat

    let private authStringsGetter: PRR.Domain.Tenant.Models.IAuthStringsGetter =
        { ClientId = fun () -> getRandomString 33
          ClientSecret = fun () -> getRandomString 50
          AuthorizationCode = fun () -> getRandomString 35
          HS256SigningSecret = fun () -> getRandomString 35
          RS256XMLParams =
              fun () ->
                  let rsa = RSA.Create(2048)
                  rsa.ToXmlString(true) }

    let private createUserTenant (ctx: HttpContext) userId userEmail =

        let configuration = ctx.GetService<IConfiguration>()

        let authConfig: PRR.Domain.Tenant.Models.AuthConfig =
            { // AccessTokenSecret = configuration.GetValue<string>("TenantAuth:AccessTokenSecret")
              IdTokenExpiresIn = configuration.GetValue<int<minutes>>("TenantAuth:IdTokenExpiresInMinutes")
              AccessTokenExpiresIn = configuration.GetValue<int<minutes>>("TenantAuth:AccessTokenExpiresInMinutes")
              RefreshTokenExpiresIn = configuration.GetValue<int<minutes>>("TenantAuth:RefreshTokenExpiresInMinutes") }


        // Code for which PRR.Domain.Tenant was included not allowed to use PRR.Domain.Tenant anywhere else in this project !!!
        let env: PRR.Domain.Tenant.CreateUserTenant.Env =
            { DbDataContext = getDataContext ctx
              AuthConfig = authConfig
              AuthStringsGetter = authStringsGetter }

        PRR.Domain.Tenant.CreateUserTenant.createUserTenant env { UserId = userId; Email = userEmail }
    //

    let private dropDatabase (connectionString: string) (dbName: string) =
        let client = MongoClient(connectionString)
        client.DropDatabase(dbName)

    let private recreateMongoDb (ctx: HttpContext) =
        let config = ctx.GetService<IConfiguration>()

        dropDatabase
            (config.GetValue "MongoKeyValueStorage:ConnectionString")
            (config.GetValue "MongoKeyValueStorage:DbName")

        dropDatabase (config.GetValue "MongoViewStorage:ConnectionString") (config.GetValue "MongoViewStorage:DbName")

    let private recreatedDataContextDb ctx =
        let dataContext = getDataContext ctx
        dataContext.Database.EnsureDeleted() |> ignore
        dataContext.Database.Migrate() |> ignore

    let private recreatedDbs ctx =
        recreatedDataContextDb ctx
        recreateMongoDb ctx

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

                         Thread.Sleep(1000)

                         // signup
                         let signUpConfirmItem: SignUpKV =
                             { FirstName = "test"
                               LastName = "user"
                               Email = "hahijo5833@acceptmail.net"
                               Password = (getPasswordSalter ctx) "#6VvR&^"
                               Token = ""
                               ExpiredAt = DateTime.UtcNow.AddDays(1.)
                               RedirectUri = null
                               ExistentUserId = None }


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
                         let loginEnv: PRR.Domain.Auth.LogIn.Common.SignInUserEnv =
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

                             let! _ = createUserTenant ctx userId signUpConfirmItem.Email

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
                                                 (dur
                                                     .Domain
                                                     .Applications
                                                     .Single(fun p -> p.IsDomainManagement)
                                                     .ClientId)
                                     }
                                     |> toSingleAsync
                                 | _ -> Task.FromResult "__DEFAULT_CLIENT_ID__"

                             //
                             let logInEnv =
                                 PostToken.getTokenResourceOwnerPasswordEnv ctx

                             let logInData: PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword.Models.Data =
                                 { Grant_Type = "password"
                                   Client_Id = clientId
                                   Username = signUpConfirmItem.Email
                                   Password = signUpConfirmItem.Password
                                   Scope = "openid email profile" }

                             let! res =
                                 PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword.TokenResourceOwnerPassword.tokenResourceOwnerPassword
                                     logInEnv
                                     logInData

                             let result = res

                             ctx.Response.Cookies.Delete("sso")

                             return! Successful.OK result next ctx
                         } ]
#endif
