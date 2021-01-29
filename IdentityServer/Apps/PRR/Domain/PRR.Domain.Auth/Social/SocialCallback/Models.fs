namespace PRR.Domain.Auth.Social.SocialCallback

open PRR.Domain.Models
open DataAvail.HttpRequest.Core
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext

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
          HttpRequestFun: HttpRequestFun
          PerimeterSocialClientSecretKeys: PerimeterSocialClientSecretKeys
          SocialCallbackUrl: string
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type Result =
        { RedirectUrl: Uri
          SocialLoginToken: Token }
