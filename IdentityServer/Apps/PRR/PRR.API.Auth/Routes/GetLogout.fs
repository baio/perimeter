namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.LogOut
open Microsoft.Extensions.Logging
open DataAvail.Http.Exceptions
open PRR.API.Auth.Routes.Helpers

// https://identityserver4.readthedocs.io/en/latest/endpoints/endsession.html
module internal GetLogout =

    let getEnv ctx =
        { DataContext = getDataContext ctx
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx
          JwtConfig = (getConfig ctx).Auth.Jwt }

    let handler next ctx =

        let env = getEnv ctx

        let logger = env.Logger

        let mutable returnUri': string = null

        task {

            let returnUri =
                match bindQueryStringField "post_logout_redirect_uri" ctx with
                | Some returnUri ->
                    returnUri' <- returnUri
                    returnUri
                | None ->
                    logger.LogWarning("post_logout_redirect_uri param is not found")
                    raise (unAuthorized "post_logout_redirect_uri param is not found")

            let accessToken =
                match bindQueryStringField "id_token_hint" ctx with
                | Some accessToken -> accessToken
                | None ->
                    logger.LogWarning("id_token_hint param is not found")
                    raise (unAuthorized "id_token_hint param is not found")

            let data: Data =
                { ReturnUri = returnUri
                  IdToken = accessToken }

            try

                let! result = logout env data

                ctx.Response.Cookies.Delete("sso")

                return! redirectTo false result.ReturnUri next ctx

            with ex ->

                let refererUrl = getRefererUrl ctx

                let redirectUrlError =
                    getExnRedirectUrl (refererUrl, returnUri') ex

                env.Logger.LogWarning("Redirect on ${@error} to ${redirectUrlError}", ex, redirectUrlError)

                return! redirectTo false redirectUrlError next ctx
        }
