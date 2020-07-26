namespace PRR.API.Tests.Utils

open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth
open System.Threading


[<AutoOpen>]
module CreateUser =

    [<CLIMutable>]
    type SignInResult =
        { idToken: string
          accessToken: string
          refreshToken: string }

    let createUser' signInUnderSampleDoamin (env: UserTestContext) (userData: SignUp.Models.Data) password =
        task {
            let! _ = env.TestFixture.HttpPostAsync' "/auth/sign-up" userData
            env.ConfirmTokenWaitHandle.WaitOne() |> ignore
            let confirmData: SignUpConfirm.Models.Data =
                { Token = env.GetConfirmToken()
                  Password = password }

            let! _ = env.TestFixture.HttpPostAsync' "/auth/sign-up/confirm" confirmData

            env.TenantWaitHandle.WaitOne() |> ignore

            let tenant = env.GetTenant()

            let signInData: SignIn.Models.SignInData =
                { Email = userData.Email
                  Password = password
                  ClientId =
                      if signInUnderSampleDoamin then tenant.SampleApplicationClientId
                      else tenant.TenantManagementApplicationClientId }

            let! result = env.TestFixture.HttpPostAsync' "/auth/sign-in" signInData

            let! result = readAsJsonAsync<SignInResult> result

            return result.accessToken
        }
        
    let createUser = createUser' true
        
        