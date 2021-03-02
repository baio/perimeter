namespace PRR.Domain.Auth.LogIn.Common

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
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
      Nonce: string
      Redirect_Uri: Uri
      Scope: Scope
      Email: string
      Password: string
      Code_Challenge: string
      Code_Challenge_Method: string
      Prompt: string option }


type AuthorizeEnv =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          PasswordSalter: StringSalter
          CodeExpiresIn: int<minutes>
          SSOExpiresIn: int<minutes>
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }
    