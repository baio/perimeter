namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

open System.Threading.Tasks
open PRR.Domain.Auth.LogIn.Common

[<AutoOpen>]
module Models =

    type SSOCookieStatus =
        | SSOCookieExists
        | SSOCookieNotExists

    type RedirectResult =
        | RedirectEmptyLoginPassword of string * SSOCookieStatus
        | RedirectNoPromptSSO of string * SSOCookieStatus
        | RedirectRegularSuccess of string
        | RedirectError of string

    type AuthorizeDispatcher = AuthorizeEnv -> string option -> AuthorizeData -> Task<RedirectResult>
