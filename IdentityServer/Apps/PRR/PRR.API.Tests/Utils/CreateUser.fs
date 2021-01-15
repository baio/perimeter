﻿namespace PRR.API.Tests.Utils

open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Microsoft.Azure.Documents
open PRR.API.Infra
open PRR.Domain.Auth
open System
open System.Security.Cryptography
open System.Threading
open System.Web


[<AutoOpen>]
module CreateUser =

    let logIn' (testFixture: TestFixture) (data: PRR.Domain.Auth.LogIn.Models.Data) =
        testFixture.HttpPostFormAsync'
            "/api/auth/login"
            (Map
                (seq {
                    ("Client_Id", data.Client_Id)
                    ("Response_Type", data.Response_Type)
                    ("State", data.State)
                    ("Redirect_Uri", data.Redirect_Uri)
                    ("Scope", data.Scope)
                    ("Email", data.Email)
                    ("Password", data.Password)
                    ("Code_Challenge", data.Code_Challenge)
                    ("Code_Challenge_Method", data.Code_Challenge_Method)
                 }))

    let logIn (testFixture: TestFixture) (data: PRR.Domain.Auth.LogIn.Models.Data) =
        task {
            let! result = logIn' testFixture data
            let location = readResponseHader "Location" result
            let uri = Uri(location)
            return HttpUtility.ParseQueryString(uri.Query).Get("code")
        }

    let sha256 = SHA256.Create()
    let random = Random()



    [<CLIMutable>]
    type SignInResult =
        { idToken: string
          accessToken: string
          refreshToken: string }

    let private randomString length =

        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~"

        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat

    let private createCodeChallenge () =

        let codeVerifier = randomString 128

        (codeVerifier,
         (SHA256.getSha256Base64Hash sha256 codeVerifier)
         |> LogInToken.LogInToken.cleanupCodeChallenge)

    let logInUser' scopes (fixture: TestFixture) (clientId: string) (email: string) (password: string) =
        task {
            let redirectUri = "http://localhost:4200"

            let (codeVerifier, codeChallenge) = createCodeChallenge ()

            let clientId = clientId

            let scope =
                [ "openid"; "profile"; "email" ]
                |> Seq.append scopes
                |> String.concat " "               

            let logInData: PRR.Domain.Auth.LogIn.Models.Data =
                { Client_Id = clientId
                  Response_Type = "code"
                  State = "state"
                  Redirect_Uri = redirectUri
                  Scope = scope
                  Email = email
                  Password = password
                  Code_Challenge = codeChallenge
                  Code_Challenge_Method = "S256" }

            let! code = logIn fixture logInData

            let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                { Grant_Type = "code"
                  Code = code
                  Redirect_Uri = logInData.Redirect_Uri
                  Client_Id = clientId
                  Code_Verifier = codeVerifier }

            let! result = fixture.HttpPostAsync' "/api/auth/token" loginTokenData

            let! result =
                result
                |> readAsJsonAsync<PRR.Domain.Auth.LogInToken.Models.Result>

            return result
        }

    let logInUser = logInUser' Seq.empty

    let createUser'' signInUnderSampleDomain (env: UserTestContext) (userData: SignUp.Models.Data) =
        task {
            let! _ = env.TestFixture.HttpPostAsync' "/api/auth/sign-up" userData
            env.ConfirmTokenWaitHandle.WaitOne() |> ignore
            let confirmData: SignUpConfirm.Models.Data = { Token = env.GetConfirmToken() }

            let! _ = env.TestFixture.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

            env.TenantWaitHandle.WaitOne() |> ignore

            let tenant = env.GetTenant()

            let clientId =
                if signInUnderSampleDomain
                then tenant.DomainManagementApplicationClientId
                else tenant.TenantManagementApplicationClientId

            let! result = logInUser env.TestFixture clientId userData.Email userData.Password

            return result
        }

    let createUser' signInUnderSampleDomain b c =
        task {
            let! result = createUser'' signInUnderSampleDomain b c
            return result.access_token
        }

    let createUser = createUser' true

    let createUserNoTenant (env: UserTestContext) (userData: SignUp.Models.Data) =
        task {
            let! _ = env.TestFixture.HttpPostAsync' "/api/auth/sign-up" userData
            env.ConfirmTokenWaitHandle.WaitOne() |> ignore
            let confirmData: SignUpConfirm.Models.Data = { Token = env.GetConfirmToken() }

            let! _ = env.TestFixture.HttpPostAsync' "/api/auth/sign-up/confirm?skipCreateTenant=true" confirmData

            let clientId = "__DEFAULT_CLIENT_ID__"

            let! result = logInUser env.TestFixture clientId userData.Email userData.Password

            return result.access_token
        }
