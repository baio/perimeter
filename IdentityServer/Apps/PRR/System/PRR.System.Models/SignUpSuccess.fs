namespace PRR.System.Models

[<AutoOpen>]
module SignUpSuccess =

    type SignUpSuccess =
        { FirstName: string
          LastName: string
          Email: string
          Token: string }
