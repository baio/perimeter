namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

open System.Threading.Tasks
open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Common

[<AutoOpen>]
module Models =

    type Env =
        { AuthorizeEnv: AuthorizeEnv
          SetSSOCookie: unit -> unit
          DeleteSSOCookie: unit -> unit
          LoginPageDomain: string }

    type Data =
        { AuthorizeData: AuthorizeData
          SSOToken: string option
          RefererUrl: string }

    type AuthorizeDispatcher = Env -> Data -> Task<string>
