namespace PRR.Domain.Auth.LogIn.Social.SocialCallback.Identities

open PRR.Domain.Models
open DataAvail.HttpRequest.Core
open Newtonsoft.Json
open PRR.Data.Entities
open FSharp.Control.Tasks.V2.ContextInsensitive

// https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps


[<AutoOpen>]
module Github =

    type CodeResponse = { access_token: string }

    type UserResponse =
        { id: int
          avatar_url: string
          email: string
          name: string }

    module private Helpers =

        let getCodeResponse (httpRequestFun: HttpRequestFun) clientId secret code =
            task {
                let request: HttpRequest =
                    { Uri = "https://github.com/login/oauth/access_token"
                      Method = HttpRequestMethodPOST
                      FormBody = Seq.empty
                      QueryStringParams =
                          seq {
                              ("client_id", clientId)
                              ("client_secret", secret)
                              ("code", code)
                          }
                      Headers = seq { ("Accept", "application/json") } }

                let! content = httpRequestFun request

                return JsonConvert.DeserializeObject<CodeResponse> content
            }


        let getUserResponse (httpRequestFun: HttpRequestFun) token =
            task {

                let request: HttpRequest =
                    { Uri = "https://api.github.com/user"
                      Method = HttpRequestMethodGET
                      QueryStringParams = Seq.empty
                      FormBody = Seq.empty
                      Headers =
                          seq {
                              ("Accept", "application/json")
                              ("Authorization", (sprintf "token %s" token))
                              ("User-Agent", "Perimeter-API")
                          } }

                let! content = httpRequestFun request

                return JsonConvert.DeserializeObject<UserResponse> content
            }

        let mapSocialUserResponseToIdentity userResponse =
            let socialName = socialType2Name SocialType.Github

            SocialIdentity
                (Name = userResponse.name,
                 Email = userResponse.email,
                 SocialName = socialName,
                 SocialId = userResponse.id.ToString())

    open Helpers

    let getGithubSocialIdentity httpRequestFun socialClientId socialSecretKey code =
        task {
            // request social access token by clientId, secret and code from callback
            let! codeResponse = getCodeResponse httpRequestFun socialClientId socialSecretKey code

            // get github user by received access token
            let! userResponse = getUserResponse httpRequestFun codeResponse.access_token

            return mapSocialUserResponseToIdentity userResponse
        }
