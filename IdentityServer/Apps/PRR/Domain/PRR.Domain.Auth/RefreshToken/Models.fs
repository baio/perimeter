namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.Domain.Auth
open PRR.Domain.Auth.LogInToken
open PRR.System.Models
open System
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Data =
        { RefreshToken: string }

    type AccessToken = Token

    type RefreshToken = SignInUserEnv -> AccessToken -> RefreshToken.Item -> Task<Result * Events>
