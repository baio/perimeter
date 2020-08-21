namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.Entities
open PRR.Domain.Auth.LogInToken
open PRR.System.Models

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
                        let! res = signInUser env tokenData item.ClientId
                        return (res,
                                RefreshTokenSuccessEvent
                                    { ClientId = item.ClientId
                                      UserId = tokenData.Id
                                      RefreshToken = res.RefreshToken
                                      OldRefreshToken = item.Token })
                    | None ->
                        return! raise (UnAuthorized None)
                | _ ->
                    return! raise (UnAuthorized None)
            }
