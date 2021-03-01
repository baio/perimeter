namespace PRR.API.Auth.Routes

open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.LogIn.AuthorizeSSO
open Microsoft.Extensions.Logging

module internal PostAuthorizeSSOHandler =

    let getEnv ctx =
        { DataContext = getDataContext ctx
          CodeGenerator = getHash ctx
          CodeExpiresIn = (getConfig ctx).Auth.Jwt.CodeExpiresIn
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx }

    let handler data ctx sso =

        let env = getEnv ctx
        task {

            // let! data = bindFormAsync ctx

            try

                let! result = authorizeSSO env sso data

                let redirectUrlSuccess =
                    sprintf "%s?code=%s&state=%s" result.RedirectUri result.Code result.State

                env.Logger.LogInformation "Redirect on success ${redirectUrlSuccess}"

                return (true, redirectUrlSuccess)

            with ex ->

                let redirectUrlError =
                    sprintf "%s?error=login_required" data.Redirect_Uri

                env.Logger.LogWarning("Redirect on ${@error} to ${redirectUrlError}", ex, redirectUrlError)

                return (false, redirectUrlError)
        }
