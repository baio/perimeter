namespace PRR.API.Tests.Utils

open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Infra
open PRR.Domain.Auth
open System
open System.Security.Cryptography
open System.Threading


[<AutoOpen>]
module CreateUser =

    let sha256 = SHA256.Create()
    let random = Random()


    [<CLIMutable>]
    type SignInResult =
        { idToken: string
          accessToken: string
          refreshToken: string }

    let private randomString length =

        let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~"
        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat

    let private createCodeChallenge() =

        let codeVerfier = randomString 128

        (codeVerfier, (HashProvider.getSha256Hash sha256 codeVerfier))


    let createUser' signInUnderSampleDomain (env: UserTestContext) (userData: SignUp.Models.Data) =
        task {
            let! _ = env.TestFixture.HttpPostAsync' "/auth/sign-up" userData
            env.ConfirmTokenWaitHandle.WaitOne() |> ignore
            let confirmData: SignUpConfirm.Models.Data =
                { Token = env.GetConfirmToken() }

            let! _ = env.TestFixture.HttpPostAsync' "/auth/sign-up/confirm" confirmData

            env.TenantWaitHandle.WaitOne() |> ignore

            let tenant = env.GetTenant()

            let redirectUri = "http://localhost:4200"

            let clientId =
                if signInUnderSampleDomain then tenant.SampleApplicationClientId
                else tenant.TenantManagementApplicationClientId

            let (codeVerifier, codeChallenge) = createCodeChallenge()

            let logInData: PRR.Domain.Auth.LogIn.Models.Data =
                { Client_Id = clientId
                  Response_Type = "code"
                  State = "state"
                  Redirect_Uri = redirectUri
                  Scopes = [| "open_id"; "profile"; "email" |]
                  Email = userData.Email
                  Password = userData.Password
                  Code_Challenge = codeChallenge
                  Code_Challenge_Method = "S256" }

            let! result = env.TestFixture.HttpPostAsync' "/auth/login" logInData
            let! result = readAsJsonAsync<PRR.Domain.Auth.LogIn.Models.Result> result

            let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                { Grant_Type = "code"
                  Code = result.Code
                  Redirect_Uri = logInData.Redirect_Uri
                  Client_Id = clientId
                  Code_Verifier = codeVerifier }

            let! result = env.TestFixture.HttpPostAsync' "/auth/token" loginTokenData
            let! result = result |> readAsJsonAsync<PRR.Domain.Auth.SignIn.Models.SignInResult>

            return result.AccessToken
        }

    let createUser = createUser' true
