namespace PRR.System.Models

open Common.Domain.Models

[<AutoOpen>]
module SignUpConfirmSuccess =
    
    type SignUpConfirmSuccess = {
        UserId: UserId
        Email: string
    }
        

