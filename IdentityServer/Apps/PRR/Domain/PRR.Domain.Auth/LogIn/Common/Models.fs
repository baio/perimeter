namespace PRR.Domain.Auth.LogIn.Common

open PRR.Domain.Models

type LogInResult =
    { refresh_token: string
      access_token: string
      id_token: string }

type SignInScopes =
    | RequestedScopes of string seq
    | ValidatedScopes of AudienceScopes seq

[<CLIMutable>]
type AuthorizeData =
    { Client_Id: ClientId
      Response_Type: string
      State: string
      Redirect_Uri: Uri
      Scope: Scope
      Email: string
      Password: string
      Code_Challenge: string
      Code_Challenge_Method: string
      Prompt: string option }
