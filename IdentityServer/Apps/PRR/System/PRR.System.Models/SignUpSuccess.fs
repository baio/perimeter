namespace PRR.System.Models

[<AutoOpen>]
module SignUpSuccess =

    type SignUpSuccess =
        { FirstName: string
          LastName: string
          Email: string
          Password: string
          Token: string
          QueryString: string option }
