namespace PRR.Domain.Auth.Social.SocialAuth.GetSocialRedirectUrl

open System.Net
open System.Security.Cryptography
open System.Text
open DataAvail.Http.Exceptions.Exceptions
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.HttpRequest.Core
open System
open DataAvail.Common
open DataAvail.Common.Option
open PRR.Domain.Auth.Social.OAuth1a

// https://developer.twitter.com/en/docs/apps/callback-urls
// https://developer.twitter.com/en/docs/authentication/oauth-1-0a/obtaining-user-access-tokens
// https://developer.twitter.com/en/docs/authentication/api-reference/request_token
// https://linvi.github.io/tweetinvi/dist/authentication/authentication-url-redirect.html

[<AutoOpen>]
module GetTwitterRedirectUrl =

    type RequestTokenResponse =
        { oauth_token: string
          oauth_token_secret: string
          oauth_callback_confirmed: bool }

    let prepareAuthHeaderValue (requestString: string)
                               (consumerKey: string)
                               (consumerSecret: string)
                               (callbackUrl: string)
                               =
        let timestamp =
            (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)))
                .TotalSeconds.ToString()

        let nonce =
            Convert.ToBase64String(Encoding.ASCII.GetBytes(timestamp.ToString()))

        let nonce = "MTYxMzU1NDY2Ni4xNDkxMDM2"
        let timestamp = "1613554666.1491036"

        let parameterString =
            sprintf
                "oauth_callback=%s&oauth_consumer_key=%s&oauth_nonce=%s&oauth_signature_method=%s&oauth_timestamp=%s&oauth_version=%s"
                (WebUtility.UrlEncode callbackUrl)
                (WebUtility.UrlEncode consumerKey)
                (WebUtility.UrlEncode nonce)
                (WebUtility.UrlEncode "HMAC-SHA1")
                (WebUtility.UrlEncode(timestamp))
                (WebUtility.UrlEncode "1.0")

        let signatureBaseString =
            "POST&"
            + WebUtility.UrlEncode(requestString)
            + "&"
            + WebUtility.UrlEncode(parameterString)

        let signingKey =
            WebUtility.UrlEncode(consumerSecret) + "&"

        let signatureBaseStringBytes =
            Encoding.ASCII.GetBytes(signatureBaseString)

        let signingKeyBytes = Encoding.ASCII.GetBytes(signingKey)

        let hmacSha1 = new HMACSHA1(signingKeyBytes)

        let signature =
            hmacSha1.ComputeHash signatureBaseStringBytes

        let base64Signature = Convert.ToBase64String signature

        let authenticationHeaderValue =
            sprintf
                "OAuth oauth_callback=\"%s\", oauth_consumer_key=\"%s\", oauth_nonce=\"%s\", oauth_signature_method=\"%s\", oauth_timestamp=\"%s\", oauth_signature=\"%s\", oauth_version=\"%s\""
                (WebUtility.UrlEncode callbackUrl)
                (WebUtility.UrlEncode consumerKey)
                (WebUtility.UrlEncode nonce)
                (WebUtility.UrlEncode "HMAC-SHA1")
                (WebUtility.UrlEncode(timestamp.ToString()))
                (WebUtility.UrlEncode base64Signature)
                (WebUtility.UrlEncode "1.0")

        authenticationHeaderValue


    let private requestToken (logger: ILogger)
                             (httpRequestFun: HttpRequestFun)
                             (callbackUrl: string)
                             (consumerKey: string)
                             (consumerSecret: string)
                             =

        let uri =
            "https://api.twitter.com/oauth/request_token"

        task {

            let authHeader =
                signAuthorizationHeader
                    "POST"
                    uri
                    consumerSecret
                    ([ "oauth_callback", callbackUrl
                       "oauth_consumer_key", consumerKey ])
                    ", "
                    None

            let authHeader2 =
                prepareAuthHeaderValue uri consumerKey consumerSecret callbackUrl

            printfn "111 %s" authHeader
            printfn "222 %s" authHeader2

            let request: HttpRequest =
                { Uri = uri
                  Method = HttpRequestMethodPOST
                  FormBody = Seq.empty
                  QueryStringParams = Seq.empty
                  Headers =
                      seq {
                          ("Content-Type", "application/x-www-form-urlencoded")
                          ("Authorization", authHeader)
                      } }

            logger.LogDebug("OAuth1a request_token ${@request} ready", request)

            let! content = httpRequestFun request

            logger.LogDebug("OAuth1a request_token response ${@content}", content)

            let spts =
                content |> split '&' |> Array.map (splitTuple '=')

            let result =
                maybe {
                    let! oauth_token = tryFindTupleValue "oauth_token" spts
                    let! oauth_token_secret = tryFindTupleValue "oauth_token_secret" spts
                    let! oauth_callback_confirmed = tryFindTupleBoolean "oauth_callback_confirmed" spts

                    return
                        { oauth_token = oauth_token
                          oauth_token_secret = oauth_token_secret
                          oauth_callback_confirmed = oauth_callback_confirmed }
                }

            match result with
            | Some result ->
                logger.LogDebug("OAuth1a request_token result ${@result}", result)
                return result
            | None ->
                logger.LogError("Response from OAuth1a request_token API has unexpected data ${@content}", content)
                return raise (unexpected (sprintf "Response from OAuth1a API has unexpected data %s" content))

        }

    let getTwitterRedirectUrl (logger: ILogger) (httpRequestFun: HttpRequestFun) callbackUri clientKey socialSecretKey =
        task {
            let! tokenResponse = requestToken logger httpRequestFun callbackUri clientKey socialSecretKey

            if not tokenResponse.oauth_callback_confirmed
            then return raise (unAuthorized "oauth_callback_confirmed is false")

            let redirectUtl =
                sprintf "https://api.twitter.com/oauth/authorize?oauth_token=%s" tokenResponse.oauth_token

            logger.LogDebug("OAuth1a redirectUrl ${redirectUrl} ready", redirectUtl)

            return (tokenResponse.oauth_token, redirectUtl)
        }
