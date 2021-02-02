namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.LogOut
open Microsoft.Extensions.Logging
open DataAvail.Http.Exceptions
open PRR.API.Auth.Routes.Helpers

module internal GetLogout =

    let getEnv ctx =
        { DataContext = getDataContext ctx
          AccessTokenSecret = (getConfig ctx).Auth.Jwt.AccessTokenSecret
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx }

    let handler next ctx =

        let env = getEnv ctx

        let logger = env.Logger

        let mutable returnUri': string = null

        task {

            let returnUri =
                match bindQueryStringField "return_uri" ctx with
                | Some returnUri ->
                    returnUri' <- returnUri
                    returnUri
                | None ->
                    logger.LogWarning("return_uri param is not found")
                    raise (unAuthorized "return_uri param is not found")

            let accessToken =
                match bindQueryStringField "access_token" ctx with
                | Some accessToken -> accessToken
                | None ->
                    logger.LogWarning("access_token param is not found")
                    raise (unAuthorized "access_token param is not found")

            let data: Data =
                { ReturnUri = returnUri
                  AccessToken = accessToken }

            try

                let! result = logout env data

                return! redirectTo false result.ReturnUri next ctx

            with ex ->

                let refererUrl = getRefererUrl ctx

                let redirectUrlError =
                    getExnRedirectUrl (refererUrl, returnUri') ex

                env.Logger.LogWarning("Redirect on ${@error} to ${redirectUrlError}", ex, redirectUrlError)

                return! redirectTo false redirectUrlError next ctx
        }
