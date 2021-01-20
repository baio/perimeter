namespace PRR.API.Configuration

open Common.Domain.Models.Models
open Microsoft.Extensions.DependencyInjection
open PRR.API.Infra
open PRR.API.Infra.Mail
open PRR.Domain.Auth
open PRR.System.Models
open System.Security.Cryptography
open PRR.System

type ActorsConnectionsString =
    { MongoJournal: string
      MongoSnapshot: string
      MongoViews: string }

type ActorsConfig =
    { Mail: MailEnv
      SendGridApiKey: string
      Jwt: JwtConfig
      SignUpTokenExpiresIn: int<minutes>
      ResetPasswordTokenExpiresIn: int<minutes>
      ConnectionStrings: ActorsConnectionsString
      EnvironmentName: string }

[<AutoOpen>]
module private Actors =

    let configureActors (config: ActorsConfig) (services: IServiceCollection) =

        let sha256 = SHA256.Create()
        let hashProvider = HashProvider sha256

        let mailEnv: MailEnv =
            { FromEmail = config.Mail.FromEmail
              FromName = config.Mail.FromName
              Project =
                  { Name = config.Mail.Project.Name
                    BaseUrl = config.Mail.Project.BaseUrl
                    ConfirmSignUpUrl = config.Mail.Project.ConfirmSignUpUrl
                    ResetPasswordUrl = config.Mail.Project.ResetPasswordUrl } }

        let sendGridApiKey = config.SendGridApiKey

        let mailSender =
            SendGridMail.createSendMail sendGridApiKey

        let systemEnv: SystemEnv =
            let serviceProvider = services.BuildServiceProvider()
            { SendMail = createSendMail mailEnv mailSender
              GetDataContextProvider =
                  fun () -> new DataContextProvider(serviceProvider.CreateScope()) :> IDataContextProvider
              HashProvider = (hashProvider :> IHashProvider).GetHash
              PasswordSalter = serviceProvider.GetService<IPasswordSaltProvider>().SaltPassword
              AuthStringsProvider = serviceProvider.GetService<IAuthStringsProvider>().AuthStringsProvider
              AuthConfig =
                  { AccessTokenSecret = config.Jwt.AccessTokenSecret
                    IdTokenExpiresIn = config.Jwt.IdTokenExpiresIn
                    AccessTokenExpiresIn = config.Jwt.AccessTokenExpiresIn
                    RefreshTokenExpiresIn = config.Jwt.RefreshTokenExpiresIn
                    SignUpTokenExpiresIn = config.SignUpTokenExpiresIn
                    ResetPasswordTokenExpiresIn = config.ResetPasswordTokenExpiresIn }
              EventHandledCallback = fun _ -> () }

        let systemConfig =
            { JournalConnectionString = config.ConnectionStrings.MongoJournal
              SnapshotConnectionString = config.ConnectionStrings.MongoSnapshot
              ViewsConnectionString = config.ConnectionStrings.MongoViews }

#if TEST
        // Tests must initialize sys by themselves
        //For tests
        services.AddSingleton<SystemEnv>(fun _ -> systemEnv)
        |> ignore
        services.AddSingleton<SystemConfig>(fun _ -> systemConfig)
        |> ignore
#else
        let env = config.EnvironmentName

        let akkaConfFile = sprintf "akka.%s.hocon" env

        printfn "Akka conf file %s" akkaConfFile

        let sys =
            setUp systemEnv systemConfig akkaConfFile

        services.AddSingleton<ICQRSSystem>(fun _ -> sys)
        |> ignore

        let sysConfig: PRR.Sys.SetUp.Config =
            { JournalConnectionString = config.ConnectionStrings.MongoJournal
              SnapshotConnectionString = config.ConnectionStrings.MongoSnapshot }

        let sys1 =
            PRR.Sys.SetUp.setUp sysConfig akkaConfFile

        services.AddSingleton<ISystemActorsProvider>(SystemActorsProvider sys1)
        |> ignore
#endif
