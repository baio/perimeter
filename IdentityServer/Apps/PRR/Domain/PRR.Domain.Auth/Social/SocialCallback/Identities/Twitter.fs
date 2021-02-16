namespace PRR.Domain.Auth.Social.SocialCallback.Identities

open DataAvail.Http.Exceptions.Exceptions
open Microsoft.Extensions.Logging
open PRR.Domain.Models
open DataAvail.HttpRequest.Core
open Newtonsoft.Json
open PRR.Data.Entities
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.Common

// https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/get-account-verify_credentials

[<AutoOpen>]
module Twitter =

    type AccessTokenResponse =
        { oauth_token: string
          oauth_token_secret: string }

    type VerifyCredentialsResponse =
        { email: string
          name: string
          id_str: string }

    module private Helpers =

        let accessTokenRequest (logger: ILogger) (httpRequestFun: HttpRequestFun) consumerKey token verifier =
            task {

                let request: HttpRequest =
                    { Uri = "https://api.twitter.com/oauth/access_token"
                      Method = HttpRequestMethodPOST
                      QueryStringParams = Seq.empty
                      FormBody =
                          seq {
                              ("oauth_consumer_key", consumerKey)
                              ("oauth_verifier", verifier)
                          }
                      Headers =
                          seq {
                              ("Accept", "application/json")
                              ("oauth_token", token)
                          } }

                logger.LogDebug("OAuth1a access_token request ${@request} is ready", request)

                let! content = httpRequestFun request
                logger.LogDebug("OAuth1a access_token response ${content}", content)
                return JsonConvert.DeserializeObject<AccessTokenResponse> content
            }

        let verifyCredentialsRequest (logger: ILogger) (httpRequestFun: HttpRequestFun) consumerKey token =
            task {

                let request: HttpRequest =
                    { Uri = "https://api.twitter.com/1.1/account/verify_credentials.json"
                      Method = HttpRequestMethodGET
                      QueryStringParams =
                          seq {
                              ("oauth_consumer_key", consumerKey)
                              ("oauth_token", token)
                              ("skip_status", "true")
                              ("include_email", "true")
                          }
                      FormBody = Seq.empty
                      Headers = seq { ("Accept", "application/json") } }

                logger.LogDebug("OAuth1a verify_credentials request ${@request} is ready", request)

                let! content = httpRequestFun request

                logger.LogDebug("OAuth1a verify_credentials response ${content}", content)

                return JsonConvert.DeserializeObject<VerifyCredentialsResponse> content
            }


        let mapSocialUserResponseToIdentity userResponse =

            let socialName = socialType2Name SocialType.Twitter

            if (isEmpty userResponse.email)
            then raise (unAuthorized "Email should be set and confirmed")

            SocialIdentity
                (Name = emptyDefault userResponse.email userResponse.name,
                 Email = userResponse.email,
                 SocialName = socialName,
                 SocialId = userResponse.id_str)

    open Helpers

    let getTwitterSocialIdentity (logger: ILogger) httpRequestFun socialSecretKey state code =
        task {

            let! accessTokenResponse = accessTokenRequest logger httpRequestFun socialSecretKey code state

            let! verifyCredentialsResponse =
                verifyCredentialsRequest logger httpRequestFun socialSecretKey accessTokenResponse.oauth_token

            logger.LogDebug("OAuth1a map to social user ${@data}", verifyCredentialsResponse)
            return mapSocialUserResponseToIdentity verifyCredentialsResponse
        }
