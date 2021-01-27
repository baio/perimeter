namespace PRR.Domain.Auth.Common

open System
open DataAvail.KeyValueStorage.Core

[<AutoOpen>]
module KeyValueModels =

    [<PartitionName("SignUp")>]
    type SignUpKV =
        { FirstName: string
          LastName: string
          Email: string
          Password: string
          Token: string
          ExpiredAt: DateTime
          QueryString: string option }


    [<PartitionName("ResetPassword")>]
    type ResetPasswordKV = { Email: string }
