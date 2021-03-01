﻿namespace PRR.API.Auth.Routes

open PRR.Domain.Models
open PRR.Domain.Auth
open PRR.Domain.Auth.LogIn.Authorize
open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Auth.Routes.Helpers
open Microsoft.Extensions.Logging

module internal PostAuthorizeHandler =


    let getEnv ctx =

        let config = getConfig ctx

        { DataContext = getDataContext ctx
          PasswordSalter = getPasswordSalter ctx
          CodeGenerator = getHash ctx
          CodeExpiresIn = config.Auth.Jwt.CodeExpiresIn
          SSOExpiresIn = config.Auth.SSOCookieExpiresIn
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx }


    let handler data ctx sso =

        let env = getEnv ctx

        task {

            try

                let! result = authorize env sso data

                let redirectUrlSuccess =
                    sprintf "%s?code=%s&state=%s" result.RedirectUri result.Code result.State

                env.Logger.LogInformation "Redirect on success ${redirectUrlSuccess}"

                return redirectUrlSuccess

            with ex ->

                let refererUrl = getRefererUrl ctx

                let redirectUrlError =
                    getExnRedirectUrl (refererUrl, data.Redirect_Uri) ex

                env.Logger.LogWarning("Redirect on ${@error} to ${redirectUrlError}", ex, redirectUrlError)

                return redirectUrlError
        }
