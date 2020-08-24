namespace PRR.API.Routes

open Akkling
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.EntityFrameworkCore
open PRR.Domain.Auth.SignUpConfirm
open PRR.System.Models
open System
open System.Threading

module E2E =

    let private recreatedDb ctx =
        let dataContext = getDataContext ctx
        dataContext.Database.EnsureDeleted() |> ignore
        dataContext.Database.Migrate() |> ignore

    let createRoutes() =
        choose
            [ route "/e2e/reset" >=> POST >=> fun next ctx ->
                // Recreate db on start
                recreatedDb ctx
                Successful.NO_CONTENT next ctx

              route "/e2e/reinit" >=> POST >=> fun next ctx ->
                  let dataContext = getDataContext ctx
                  let sys = ctx.GetService<ICQRSSystem>()
                  recreatedDb ctx
                  // signup
                  let signupEnv = { DataContext = dataContext }

                  let signupItem: SignUpToken.Item =
                      { FirstName = "test"
                        LastName = "user"
                        Email = "test@user.dev"
                        Password = "123"
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
                      let! evt = signUpConfirm signupEnv signupItem
                      sys.EventsRef <! evt
                      //
                      Thread.Sleep(10)
                      let! res = PRR.Domain.Auth.LogInEmail.logInEmail loginEnv "__DEFAULT_CLIENT_ID__"
                                     signupItem.Email signupItem.Password
                                     
                      let (result, _) = res                                     
                      
                      return! Successful.OK result next ctx
                  } ]
