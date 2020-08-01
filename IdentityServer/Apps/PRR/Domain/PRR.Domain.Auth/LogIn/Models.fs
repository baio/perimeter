namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models

open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module LogIn =

    type Data =
        { ClientId: ClientId
          ResponseType: string
          State: string
          RedirectUri: Uri
          Scopes: Scope seq
          Email: string
          Password: string
          CodeChallenge: string
          CodeChallengeMethod: string }

    type Result =
        { State: string
          Code: string }

    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          PasswordSalter: StringSalter
          CodeExpiresIn: int<minutes> }

type LogIn = Env -> Data -> Task<Result * Events>
