﻿namespace PRR.API.Routes.Auth

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.LogInSSO
open Microsoft.Extensions.Logging

module internal PostLogInSSO =

    let getEnv ctx =
        { DataContext = getDataContext ctx
          CodeGenerator = getHash ctx
          CodeExpiresIn = (getConfig ctx).Auth.Jwt.CodeExpiresIn
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx }

    let handler ctx sso =

        let env = getEnv ctx
        task {

            let! data = bindFormAsync ctx

            try

                let! result = logInSSO env sso data

                let redirectUrlSuccess =
                    sprintf "%s?code=%s&state=%s" result.RedirectUri result.Code result.State

                env.Logger.LogInformation "Redirect on success ${redirectUrlSuccess}"

                return redirectUrlSuccess

            with ex ->

                let redirectUrlError =
                    sprintf "%s?error=login_required" data.Redirect_Uri

                env.Logger.LogWarning("Redirect on ${@error} to ${redirectUrlError}", ex, redirectUrlError)

                return redirectUrlError
        }