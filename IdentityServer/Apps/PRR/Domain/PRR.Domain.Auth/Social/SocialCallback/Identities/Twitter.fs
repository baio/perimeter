namespace PRR.Domain.Auth.Social.SocialCallback.Identities.Twitter

open Common.Domain.Models
open Newtonsoft.Json
open PRR.Data.Entities
open FSharp.Control.Tasks.V2.ContextInsensitive

// https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps

module public Models =

    type GithubCodeResponse = { access_token: string }

    type GithubUserResponse =
        { id: int
          avatar_url: string
          email: string
          name: string }


module private Helpers =

    open Models

    let getGithubCodeResponse (httpRequestFun: HttpRequestFun) clientId secret code =
        task {
            let request: HttpRequest =
                { Uri = "https://github.com/login/oauth/access_token"
                  Method = HttpRequestMethodPOST
                  QueryStringParams =
                      seq {
                          ("client_id", clientId)
                          ("client_secret", secret)
                          ("code", code)
                      }
                  Headers = seq { ("Accept", "application/json") } }

            let! content = httpRequestFun request

            return JsonConvert.DeserializeObject<GithubCodeResponse> content
        }


    let getGithubUserResponse (httpRequestFun: HttpRequestFun) token =
        task {

            let request: HttpRequest =
                { Uri = "https://api.github.com/user"
                  Method = HttpRequestMethodGET
                  QueryStringParams = Seq.empty
                  Headers =
                      seq {
                          ("Accept", "application/json")
                          ("Authorization", (sprintf "token %s" token))
                          ("User-Agent", "Perimeter-API")
                      } }

            let! content = httpRequestFun request

            return JsonConvert.DeserializeObject<GithubUserResponse> content
        }

    let mapSocialUserResponseToIdentity userResponse =
        let socialName = socialType2Name SocialType.Github
        SocialIdentity
            (Name = userResponse.name,
             Email = userResponse.email,
             SocialName = socialName,
             SocialId = userResponse.id.ToString())

module internal Handler =

    open Helpers

    let getSocialIdentity httpRequestFun socialClientId socialSecretKey code =
        task {
            // request social access token by clientId, secret and code from callback
            let! codeResponse = getGithubCodeResponse httpRequestFun socialClientId socialSecretKey code

            // get github user by received access token
            let! userResponse = getGithubUserResponse httpRequestFun codeResponse.access_token

            return mapSocialUserResponseToIdentity userResponse
        }
