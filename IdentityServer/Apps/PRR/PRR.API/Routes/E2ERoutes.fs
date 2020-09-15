namespace PRR.API.Routes

open Akkling
open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.EntityFrameworkCore
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

    let private recreatedDb ctx =
        let dataContext = getDataContext ctx
        dataContext.Database.EnsureDeleted() |> ignore
        dataContext.Database.Migrate() |> ignore

    let createRoutes () =
        choose [ route "/e2e/reset"
                 >=> POST
                 >=> fun next ctx ->
                         // Recreate db on start
                         recreatedDb ctx
                         Successful.NO_CONTENT next ctx

                 route "/e2e/reinit"
                 >=> POST
                 >=> fun next ctx ->
                         let dataContext = getDataContext ctx
                         let sys = ctx.GetService<ICQRSSystem>()
                         recreatedDb ctx
                         // signup
                         let signupEnv = { DataContext = dataContext }

                         let signupItem: SignUpToken.Item =
                             { FirstName = "test"
                               LastName = "user"
                               Email = "hahijo5833@acceptmail.net"
                               Password = (getPasswordSalter ctx) "#6VvR&^"
                               Token = ""
                               ExpiredAt = DateTime.UtcNow.AddDays(1.)
                               QueryString = None }
                         // login
                         let loginEnv: PRR.Domain.Auth.LogInToken.Models.Env =
                             { DataContext = getDataContext ctx
                               HashProvider = getHash ctx
                               Sha256Provider = getSHA256 ctx
                               SSOCookieExpiresIn = (getConfig ctx).SSOCookieExpiresIn
                               JwtConfig = (getConfig ctx).Jwt }

                         task {
                             let! evt = signUpConfirm true signupEnv signupItem
                             sys.EventsRef <! evt
                             //
                             Thread.Sleep(10)

                             let! data = ctx |> bindJsonAsync<ReinitData>

                             let! clientId =
                                 match data.LoginAsDomain with
                                 | true ->
                                     query {
                                         for dur in dataContext.DomainUserRole do
                                             where
                                                 (dur.RoleId = PRR.Data.DataContext.Seed.Roles.DomainOwner.Id
                                                  && dur.UserEmail = signupItem.Email)
                                             select
                                                 (dur.Domain.Applications.Single(fun p -> p.IsDomainManagement).ClientId)
                                     }
                                     |> LinqHelpers.toSingleAsync
                                 | _ -> Task.FromResult "__DEFAULT_CLIENT_ID__"


                             let! res =
                                 PRR.Domain.Auth.LogInEmail.logInEmail
                                     loginEnv
                                     clientId
                                     signupItem.Email
                                     signupItem.Password

                             let (result, _, _) = res


                             ctx.Response.Cookies.Delete("sso")

                             return! Successful.OK result next ctx
                         } ]
