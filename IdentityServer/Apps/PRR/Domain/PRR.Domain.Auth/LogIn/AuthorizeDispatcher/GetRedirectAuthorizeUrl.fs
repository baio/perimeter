namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

[<AutoOpen>]
module private GetRedirectAuthorizeUrl =

    open PRR.Domain.Auth.LogIn.Common
    open DataAvail.Common

    let getRedirectAuthorizeUrl (data: AuthorizeData) error errorDescription =

        let qs =
            [| mapNullValue "client_id" data.Client_Id
               mapOptionValue "prompt" data.Prompt
               mapNullValue "scope" data.Scope
               mapNullValue "state" data.State
               mapNullValue "nonce" data.Nonce
               mapNullValue "code_challenge" data.Code_Challenge
               mapNullValue "redirect_uri" data.Redirect_Uri
               mapNullValue "response_type" data.Response_Type
               mapNullValue "code_challenge_method" data.Code_Challenge_Method
               mapNullValue "error" error
               mapNullValue "error_description" errorDescription |]
            |> Seq.choose id
            |> joinQueryStringTuples

        sprintf "http://localhost:4200?%s" qs
