namespace PRR.Domain.Auth.Social.SocialCallback.Identities

open DataAvail.Http.Exceptions.Exceptions
open Microsoft.Extensions.Logging
open PRR.Domain.Models
open DataAvail.HttpRequest.Core
open Newtonsoft.Json
open PRR.Data.Entities
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.Common
open DataAvail.Common.Option
open PRR.Domain.Auth.Social.OAuth1a

// https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/get-account-verify_credentials
// https://stackoverflow.com/questions/45259100/twitter-api-returning-215-bad-authentication-data-when-calling-account-verify-cr
// https://developer.twitter.com/en/docs/authentication/oauth-1-0a/creating-a-signature

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

                let uri =
                    "https://api.twitter.com/oauth/access_token"

                let request: HttpRequest =
                    { Uri = uri
                      Method = HttpRequestMethodPOST
                      QueryStringParams =
                          seq {
                              ("oauth_token", token)
                              ("oauth_verifier", verifier)
                              ("oauth_consumer_key", consumerKey)
                          }
                      FormBody = Seq.empty
                      Headers =
                          seq {
                              ("Accept", "application/json")
                              ("Content-Type", "application/x-www-form-urlencoded")
                          } }

                logger.LogDebug("OAuth1a access_token request ${@request} is ready", request)

                let! content = httpRequestFun request
                logger.LogDebug("OAuth1a access_token response ${content}", content)

                let spts =
                    content |> split '&' |> Array.map (splitTuple '=')

                let result =
                    maybe {
                        let! oauth_token = tryFindTupleValue "oauth_token" spts
                        let! oauth_token_secret = tryFindTupleValue "oauth_token_secret" spts

                        return
                            { oauth_token = oauth_token
                              oauth_token_secret = oauth_token_secret }
                    }

                match result with
                | Some result ->
                    logger.LogDebug("OAuth1a access_token result ${@result}", result)
                    return result
                | None ->
                    logger.LogError("Response from OAuth1a access_token API has unexpected data ${@content}", content)

                    return
                        raise
                            (unexpected
                                (sprintf "Response from access_token OAuth1a API has unexpected data %s" content))
            }

        let verifyCredentialsRequest (logger: ILogger)
                                     (httpRequestFun: HttpRequestFun)
                                     consumerKey
                                     consumerSecret
                                     token
                                     tokenSecret
                                     =
            task {

                let uri =
                    "https://api.twitter.com/1.1/account/verify_credentials.json"

                let authHeader =
                    signAuthorizationHeader
                        "GET"
                        uri
                        (consumerKey, consumerSecret)
                        Seq.empty
                        //("skip_status", "true")
                        //("include_email", "true")
                        ", "
                        (Some(token, tokenSecret))

                let request: HttpRequest =
                    { Uri = uri
                      Method = HttpRequestMethodGET
                      QueryStringParams = Seq.empty
                      (*
                          seq {
                              ("oauth_consumer_key", consumerKey)
                              ("oauth_token", token)
                          }*)
                      FormBody = Seq.empty
                      Headers =
                          seq {
                              ("Accept", "application/json")
                              ("Authorization", authHeader)
                          }
                    //("skip_status", "true")
                    //("include_email", "true")
                    }

                logger.LogDebug("OAuth1a verify_credentials request ${@request} is ready", request)

                let! content = httpRequestFun request

                logger.LogDebug("OAuth1a verify_credentials response ${content}", content)

                return JsonConvert.DeserializeObject<VerifyCredentialsResponse> content
            //("skip_status", "true")
            //("include_email", "true")
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

    let getTwitterSocialIdentity (logger: ILogger) httpRequestFun socialClientKey socialClientSecret state code =
        task {

            let! accessTokenResponse = accessTokenRequest logger httpRequestFun socialClientSecret code state

            let! verifyCredentialsResponse =
                verifyCredentialsRequest
                    logger
                    httpRequestFun
                    socialClientKey
                    socialClientSecret
                    accessTokenResponse.oauth_token
                    accessTokenResponse.oauth_token_secret

            logger.LogDebug("OAuth1a map to social user ${@data}", verifyCredentialsResponse)
            return mapSocialUserResponseToIdentity verifyCredentialsResponse
        }
