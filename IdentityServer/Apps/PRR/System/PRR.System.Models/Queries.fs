namespace PRR.System.Models

open Akkling
open Common.Domain.Models

[<AutoOpen>]
module Queries =
    type Queries =
        | RefreshToken of RefreshToken.Queries
        | SignUpToken of SignUpToken.Queries
        | ResetPassword of ResetPassword.Queries
        | LogIn of LogIn.Queries
        | SSO of SSO.Queries
