namespace PRR.API.Routes.Auth.SocialCallback

open Common.Domain.Giraffe
open Common.Domain.Models
open Microsoft.AspNetCore.Http
open PRR.API.Routes.DIHelpers
open PRR.API
open Common.Utils
open FSharp.Akkling.CQRS
open PRR.Sys.Models.Social

[<AutoOpen>]
module private GetSocialLoginItem =

    let getSocialLoginItem (ctx: HttpContext) (token: Token) =
        let sysActors = getSystemActors ctx
        let sys = sysActors.System
        let socialActor = sysActors.Social
        taskOfQueryActor sys socialActor (fun sendResultTo -> SocialLoginQueryCommand(token, sendResultTo))
        |> TaskUtils.map (snd)
