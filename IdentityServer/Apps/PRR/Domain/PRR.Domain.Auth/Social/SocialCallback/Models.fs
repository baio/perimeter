namespace PRR.Domain.Auth.SocialCallback

open System.Threading.Tasks
open Common.Domain.Models
open PRR.Data.DataContext
open PRR.Sys.Models.Social
open PRR.System.Models

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data = { Code: string; State: string }

    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          PasswordSalter: StringSalter
          CodeExpiresIn: int<minutes>
          SSOExpiresIn: int<minutes>
          GetSocialLoginItem: Token -> Task<Item option>
          HttpRequestFun: HttpRequestFun }

    type Result =
        { RedirectUrl: Uri
          SocialLoginToken: Token
          LoginItem: LogIn.Item
          SSOItem: SSO.Item option }
