namespace PRR.API.Auth.Routes

open DataAvail.Http.Exceptions
open DataAvail.Http.Exceptions.Exceptions
open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.LogIn
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http

module PostToken =

    type GrantTypeData = { Grant_Type: string }

    let getTokenAuthorizationCodeEnv ctx =
        let config = getConfig ctx



        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          Sha256Provider = getSHA256 ctx
          RefreshTokenExpiresIn = config.Auth.RefreshTokenExpiresIn
          JwtConfig = config.Auth.Jwt
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx
          PublishEndpoint = getPublishEndpoint ctx }: TokenAuthorizationCode.Models.Env

    let getTokenResourceOwnerPasswordEnv ctx =
        let config = getConfig ctx



        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          Sha256Provider = getSHA256 ctx
          RefreshTokenExpiresIn = config.Auth.RefreshTokenExpiresIn
          JwtConfig = config.Auth.Jwt
          Logger = getLogger ctx
          StringSalter = getPasswordSalter ctx
          KeyValueStorage = getKeyValueStorage ctx
          PublishEndpoint = getPublishEndpoint ctx }: TokenResourceOwnerPassword.Models.Env

    // This is merged version of data for any grant_type
    type Data =
        { Grant_Type: string
          Client_Id: string
          // Authorization Code
          Code: string
          Redirect_Uri: string
          Code_Verifier: string
          Client_Secret: string
          // Password
          Username: string
          Password: string
          Scope: string }

    let private handler' ctx =
        task {

            let logger = getLogger ctx

            let! data = bindJsonAsync<Data> ctx

            logger.LogDebug("Token handler with data ${@grantTypeData}", data)

            match data.Grant_Type with
            | "authorization_code" ->
                logger.LogDebug("Token handler detects authorization_code flow")
                let env = getTokenAuthorizationCodeEnv ctx

                let tokenData: TokenAuthorizationCode.Models.Data =
                    { Grant_Type = data.Grant_Type
                      Client_Id = data.Client_Id
                      Code = data.Code
                      Redirect_Uri = data.Redirect_Uri
                      Code_Verifier = data.Code_Verifier
                      Client_Secret = data.Client_Secret }

                let! result = TokenAuthorizationCode.TokenAuthorizationCode.tokenAuthorizationCode env tokenData
                return result
            | "password" ->
                logger.LogDebug("Token handler detects password flow")
                let env = getTokenResourceOwnerPasswordEnv ctx

                let tokenData: TokenResourceOwnerPassword.Models.Data =
                    { Grant_Type = data.Grant_Type
                      Client_Id = data.Client_Id
                      Username = data.Username
                      Password = data.Password
                      Scope = data.Scope }

                let! result = TokenResourceOwnerPassword.TokenResourceOwnerPassword.tokenResourceOwnerPassword env tokenData
                return result
            | grantType ->
                logger.LogError("Grant type ${grantType} is unknown", grantType)
                return raise (BadRequest([| BadRequestCommonError(sprintf "Grant type %s is unknown" grantType) |]))
        }

    let handler: HttpHandler = wrapHandlerOK handler'
