namespace PRR.Domain.Auth.LogIn.Authorize

[<AutoOpen>]
module internal Authorize =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.Logging
    open DataAvail.Http.Exceptions
    open PRR.Domain.Auth.LogIn.Common
    open DataAvail.Common

    let authorize: Authorize =
        fun env sso data ->
            let dataContext = env.DataContext

            task {
                let saltedPassword = env.PasswordSalter data.Password
                match! getUserId dataContext (data.Email, saltedPassword) with
                | Some userId ->

                    let loginData: LoginData =
                        { UserId = userId
                          ClientId = data.Client_Id
                          ResponseType = data.Response_Type
                          State = data.State
                          Nonce = data.Nonce
                          RedirectUri = data.Redirect_Uri
                          Scope = data.Scope
                          Email = data.Email
                          CodeChallenge = data.Code_Challenge
                          CodeChallengeMethod = data.Code_Challenge_Method }

                    let! result = logInUser env sso loginData None

                    let redirectUrlSuccess =
                        sprintf "%s?code=%s&state=%s" result.RedirectUri result.Code result.State

                    return redirectUrlSuccess

                | None ->
                    env.Logger.LogWarning("Wrong email or password")
                    return! raise (unAuthorized "Wrong email or password")

            }
