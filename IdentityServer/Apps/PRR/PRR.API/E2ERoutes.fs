﻿namespace PRR.API.Routes

open Akkling
open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open MongoDB.Driver
open PRR.API.Routes.Auth.SignUpConfirm
open PRR.Domain.Auth.SignUpConfirm
open PRR.System.Models
open PRR.System.Utils
open System
open System.Linq
open System.Threading
open System.Threading.Tasks

[<CLIMutable>]
type ReinitData = { LoginAsDomain: bool }

module E2E =

    let private dropDatabase (connectionString: string) =
        let client = MongoClient(connectionString)
        let dbName = connectionString.Split("/") |> Seq.last
        client.DropDatabase(dbName)

    let private recreateMongoDb (ctx: HttpContext) =
        let config = ctx.GetService<IConfiguration>()
        dropDatabase (config.GetConnectionString "MongoJournal")
        dropDatabase (config.GetConnectionString "MongoSnapshot")
        dropDatabase (config.GetConnectionString "MongoViews")

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
                         let sys = ctx.GetService<ICQRSSystem>()

                         recreatedDbs ctx
                         // signup
                         let signUpConfirmItem: SignUpToken.Item =
                             { FirstName = "test"
                               LastName = "user"
                               Email = "hahijo5833@acceptmail.net"
                               Password = (getPasswordSalter ctx) "#6VvR&^"
                               Token = ""
                               ExpiredAt = DateTime.UtcNow.AddDays(1.)
                               QueryString = None }


                         let signUpEnv = PostSignUpConfirm.getEnv true ctx

                         let signUpEnv =
                             { signUpEnv with
                                   GetTokenItem = fun _ -> Task.FromResult(Some signUpConfirmItem) }

                         // login
                         let loginEnv: PRR.Domain.Auth.LogInToken.SignInUserEnv =
                             let config = getConfig ctx
                             { DataContext = getDataContext ctx
                               HashProvider = getHash ctx
                               JwtConfig = config.Auth.Jwt
                               Logger = getLogger ctx }

                         task {
                             do! signUpConfirm signUpEnv { Token = "xxx" }
                             //
                             Thread.Sleep(100)

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
                                     |> LinqHelpers.toSingleAsync
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