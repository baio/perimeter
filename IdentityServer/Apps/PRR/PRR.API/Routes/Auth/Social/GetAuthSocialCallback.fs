namespace PRR.API.Routes.AuthSocial

open Akkling
open FSharpx
open PRR.System.Models
open Common.Domain.Models
open Microsoft.AspNetCore.Http
open Giraffe
open PRR.API.Routes
open Common.Domain.Giraffe
open Common.Utils
open FSharp.Akkling.CQRS
open PRR.Domain.Auth.Social.SocialCallback
open PRR.Sys.Models.Social

module private Handler =

    let getSocialLoginItem (ctx: HttpContext) (token: Token) =
        let sysActors = getSystemActors ctx
        let sys = sysActors.System
        let socialActor = sysActors.Social
        taskOfQueryActor sys socialActor (fun sendResultTo -> SocialLoginQueryCommand(token, sendResultTo))
        |> TaskUtils.map (snd)

    let getEnv (ctx: HttpContext): Env =
        let config = getConfig ctx
        { DataContext = getDataContext ctx
          PasswordSalter = getPasswordSalter ctx
          CodeGenerator = getHash ctx
          CodeExpiresIn = config.Auth.Jwt.CodeExpiresIn
          SSOExpiresIn = config.Auth.SSOCookieExpiresIn
          GetSocialLoginItem = getSocialLoginItem ctx
          HttpRequestFun = getHttpRequestFun ctx
          SocialCallbackUrl = config.Auth.Social.CallbackUrl
          PerimeterSocialClientSecretKeys =
              { Github = config.Auth.PerimeterSocialProviders.Github.SecretKey
                Google = config.Auth.PerimeterSocialProviders.Google.SecretKey
                Twitter = config.Auth.PerimeterSocialProviders.Twitter.SecretKey } }

    open Reader

    let getParams =
        triplet
        <!> getEnv
        <*> bindQueryString
        <*> bindCookie "sso"

    let socialAuthResult (result: Result) =
        fun ctx next ->
            //
            let socialActor = (getSystemActors ctx).Social
            socialActor
            <! (SocialLoginRemoveCommand result.SocialLoginToken)
            //
            let sys = ctx.GetService<ICQRSSystem>()
            sys.EventsRef
            <! UserLogInSuccessEvent(result.LoginItem, result.SSOItem)
            //
            redirectTo false result.RedirectUrl next ctx

    let handle =
        (getParams
         >> socialCallback
         >> TaskUtils.map socialAuthResult)
        |> wrapHandler

module GetAuthSocialCallback =
    let createRoute () =
        route "/auth/social/callback"
        >=> GET
        >=> Handler.handle
