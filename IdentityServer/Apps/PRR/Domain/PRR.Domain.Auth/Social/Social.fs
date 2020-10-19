namespace PRR.Domain.Auth.Social

open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open Common.Domain.Utils
open PRR.Sys.Models.Social

[<AutoOpen>]
module Social =

    type private SocialInfo =
        { ClientId: string
          Attributes: string seq
          Permissions: string seq }

    let private getSocialConnectionInfo (dataContext: DbDataContext) clientId socialType =
        query {
            for app in dataContext.Applications do
                join sc in dataContext.SocialConnections on (app.DomainId = sc.DomainId)
                where (app.ClientId = clientId && sc.Name = socialType)
                select
                    { ClientId = sc.ClientId
                      Attributes = sc.Attributes
                      Permissions = sc.Permissions }
        }
        |> toSingleAsync


    let private getRedirectUrl token callbackUri (info: SocialInfo) =
        function
        | Github ->
            // https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps
            sprintf
                "https://github.com/login/oauth/authorize?client_id=%s&redirect_uri=%s&state=%s"
                info.ClientId
                callbackUri
                token

    let private socialName2Type socialName =
        match socialName with
        | "github" -> Github
        | _ ->
            raise
                (sprintf "Social [%s] is not found" socialName
                 |> exn)

    let socialAuth (env: Env, data: Data) =
        task {
            // get domain social info by type
            let socialType = socialName2Type data.SocialName

            printfn "+++ %s %s" data.ClientId data.SocialName
            
            let! info = getSocialConnectionInfo env.DataContext data.ClientId data.SocialName

            let token = env.HashProvider()

            let callbackUri = "http://localhost:4200/login/callback"

            let redirectUrl =
                getRedirectUrl token callbackUri info socialType

            let cmd =
                { Token = token
                  ClientId = info.ClientId
                  Type = socialType }
                |> SocialLoginAddCommand

            return (redirectUrl, cmd)
        }
