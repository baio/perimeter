namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.Entities
open PRR.Domain.Auth.LogInToken
open PRR.System.Models
open PRR.Domain.Auth.ValidateScopes

[<AutoOpen>]
module RefreshToken =

    let private getUserDataForToken dataContext userId =
        getUserDataForToken' dataContext <@ fun (user: User) -> (user.Id = userId) @>

    let refreshToken: RefreshToken =
        fun env accessToken item ->
            task {
                match validate env accessToken (item.ExpiresAt, item.UserId) with
                | Success ->
                    match! getUserDataForToken env.DataContext item.UserId with
                    | Some tokenData ->

                        // TODO : When available scopes changed while refreshing tokens what to do ?
                        // Now just silently change scopes
                        let scopes = item.Scopes
                       
                        let! (res, clientId) = signInUser env tokenData item.ClientId (RequestedScopes scopes)

                        return (res,
                                RefreshTokenSuccessEvent
                                    { ClientId = clientId
                                      UserId = tokenData.Id
                                      RefreshToken = res.refresh_token
                                      OldRefreshToken = item.Token
                                      Scopes = item.Scopes })
                    | None -> return! raise (UnAuthorized None)
                | _ -> return! raise (UnAuthorized None)
            }
