namespace PRR.Domain.Auth.LogIn.TokenClientCredentials

open PRR.Domain.Models.Models

[<AutoOpen>]
module TokenClientCredentials =

    open System
    open DataAvail.Common
    open PRR.Data.DataContext
    open PRR.Domain.Auth.LogIn.Common
    open DataAvail.Http.Exceptions
    open Microsoft.Extensions.Logging
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open DataAvail.EntityFramework.Common.LinqHelpers
    open PRR.Domain.Auth.Common
    open PRR.Domain.Auth
    open System.Linq


    let private validateData (data: Data): BadRequestError array =
        [| (validateNullOrEmpty "client_id" data.Client_Id)
           (validateNullOrEmpty "grant_type" data.Grant_Type)
           (validateContains [| "client_credentials" |] "grant_ype" data.Grant_Type)
           (validateNullOrEmpty "client_secret" data.Client_Secret)
           (validateNullOrEmpty "audience" data.Audience) |]
        |> Array.choose id

    let private getAudienceScopes (dataContext: DbDataContext) audience =
        (query {
            for api in dataContext.Apis do
                where (api.Identifier = audience)
                select api
         })
            .SelectMany(fun api -> api.Permissions.Select(fun permission -> permission.Name))
        |> toListAsync

    // https://auth0.com/docs/api/authentication?http#client-credentials-flow
    let tokenClientCredentials: TokenClientCredentials =
        fun env data ->

            let logger = env.Logger

            logger.LogDebug("TokenClientCredentials with ${@data}", data)

            task {
                let validationResult = validateData data

                if Seq.length validationResult > 0 then
                    logger.LogWarning("Validation error ${@data}", validationResult)
                    raise (BadRequest validationResult)

                let! scopes = getAudienceScopes env.DataContext data.Audience

                let audienceScopes =
                    [| { Scopes = scopes
                         Audience = data.Audience } |]

                env.Logger.LogDebug("Audience scopes ${@audienceScopes}", audienceScopes)

                let env': GetDomainSecretAndExpireEnv =
                    { JwtConfig = env.JwtConfig
                      DataContext = env.DataContext }

                let! appInfo = getClientAppInfo env.DataContext data.Client_Id

                env.Logger.LogDebug("AppInfo found ${@appInfo}", appInfo)

                let! secretData = getDomainSecretAndExpire env' appInfo.Issuer (appInfo.Type <> AppType.Regular)

                env.Logger.LogDebug("App secrets extracted")

                let signInData =
                    { UserTokenData = None
                      ClientId = data.Client_Id
                      Issuer = appInfo.Issuer
                      AudienceScopes = audienceScopes
                      RefreshTokenProvider = None
                      AccessTokenCredentials = secretData.SigningCredentials
                      AccessTokenExpiresIn = secretData.AccessTokenExpiresIn * 1<minutes>
                      IdTokenExpiresIn = 0<minutes> }

                env.Logger.LogDebug("SignIn Data created ${@signInData}", signInData)

                let signInResult = signIn signInData

                env.Logger.LogDebug("SignIn Result", signInResult)

                let result: Result =
                    { access_token = signInResult.access_token
                      token_type = "Bearer"
                      expires_in = int signInData.AccessTokenExpiresIn }

                return result
            }
