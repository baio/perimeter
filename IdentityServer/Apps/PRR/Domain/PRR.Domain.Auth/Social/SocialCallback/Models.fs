namespace PRR.Domain.Auth.Social.SocialCallback

open System.Threading.Tasks
open Common.Domain.Models
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.Sys.Models.Social
open PRR.System.Models

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data = { Code: string; State: string }

    // Since there is no app to manage perimeter admin data itself,
    // setup social providers for perimeter runtime through the environment configuration
    type PerimeterSocialClientSecretKeys =
        { Github: string
          Twitter: string
          Google: string }

    type Env =
        { DataContext: DbDataContext
          CodeGenerator: HashProvider
          PasswordSalter: StringSalter
          CodeExpiresIn: int<minutes>
          SSOExpiresIn: int<minutes>
          GetSocialLoginItem: Token -> Task<Item option>
          HttpRequestFun: HttpRequestFun
          PerimeterSocialClientSecretKeys: PerimeterSocialClientSecretKeys
          SocialCallbackUrl: string
          Logger: ILogger }

    type Result =
        { RedirectUrl: Uri
          SocialLoginToken: Token
          LoginItem: LogIn.Item
          SSOItem: SSO.Item option }
