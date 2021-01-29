namespace PRR.Domain.Auth.UpdatePassword

open System.Threading.Tasks
open PRR.Domain.Models
open PRR.Data.DataContext

[<AutoOpen>]
module Models = 

    type Env = {
        DataContext: DbDataContext
        PasswordSalter: StringSalter
    }

    [<CLIMutable>]
    type Data = {
        OldPassword: string
        Password: string
    }    
    
    type UpdatePassword = Env -> (UserId * Data) -> Task<unit>
