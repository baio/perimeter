namespace PRR.Domain.Auth.LogIn.Common

open PRR.Domain.Models

type LogInResult =
    { refresh_token: string
      access_token: string
      id_token: string }

type SignInScopes =
    | RequestedScopes of string seq
    | ValidatedScopes of AudienceScopes seq
