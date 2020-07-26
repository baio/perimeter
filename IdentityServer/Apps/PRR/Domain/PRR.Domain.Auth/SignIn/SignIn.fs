namespace PRR.Domain.Auth.SignIn

open Common.Domain.Models
open Common.Domain.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Models
open PRR.System.Models

[<AutoOpen>]
module SignIn =

    let signIn: SignIn =
        fun env data ->
            let dataContext = env.DataContext
            task {
                let saltedPassword = env.PasswordSalter data.Password
                match! getUserDataForToken dataContext (data.Email, saltedPassword) with
                | Some tokenData ->
                    let env' =
                        { DataContext = env.DataContext
                          JwtConfig = env.JwtConfig
                          HashProvider = env.HashProvider }
                    let! res1 = signInUser env' tokenData data.ClientId
                    let res2 =
                        { ClientId = data.ClientId
                          UserId = tokenData.Id
                          RefreshToken = res1.RefreshToken }
                        |> UserSignInSuccessEvent

                    return (res1, res2)
                | None ->
                    return! raiseTask UnAuthorized
            }


    let logIn: LogIn =
        fun env data ->
            task {
                let! managmentAppClientId = query {
                                                for app in env.DataContext.Applications do
                                                    where (app.Domain.Tenant.User.Email = data.Email)
                                                    select app.ClientId
                                            }
                                            |> LinqHelpers.toSingleExnAsync UnAuthorized

                return! signIn env
                            { Email = data.Email
                              Password = data.Password
                              ClientId = managmentAppClientId }
            }
