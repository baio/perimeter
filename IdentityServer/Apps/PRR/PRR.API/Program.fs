module PRR.API.App

open Akka.Actor
open Akka.Configuration
open Akkling
open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open PRR.API
open PRR.API.Infra
open PRR.API.Infra.Mail
open PRR.API.Infra.Mail.Models
open PRR.API.Routes
open PRR.API.Routes.Tenant
open PRR.Data.DataContext
open PRR.System
open PRR.System.Models
open System
open System.Security.Cryptography

let webApp =
    choose
        [ Auth.createRoutes()
          Me.createRoutes()
          Tenant.createRoutes()
#if E2E
          E2E.createRoutes()
#endif
          setStatusCode 404 >=> text "Not Found" ]

let createDbContext (connectionString: string) =
    let optionsBuilder = DbContextOptionsBuilder<DbDataContext>()
    optionsBuilder.UseNpgsql
        (connectionString, (fun b -> b.MigrationsAssembly("PRR.Data.DataContextMigrations") |> ignore))
    DbDataContext(optionsBuilder.Options)

let configureCors (builder: CorsPolicyBuilder) =
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() |> ignore

let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseAuthentication()
        .UseAuthorization()
        .UseCors(configureCors)
        .UseGiraffe(webApp)


let configureServices (context: WebHostBuilderContext) (services: IServiceCollection) =
    let config = Infra.Config.getConfig context.Configuration ()
    // Authentication
    let issuerSigningKey =
        config.Jwt.AccessTokenSecret
        |> System.Text.Encoding.ASCII.GetBytes
        |> SymmetricSecurityKey
    services.AddAuthorization() |> ignore
    services.AddAuthentication(fun options ->
            options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun x ->
            x.RequireHttpsMetadata <- false
            x.SaveToken <- true
            x.TokenValidationParameters <-
                TokenValidationParameters
                    (ValidateIssuerSigningKey = true, IssuerSigningKey = issuerSigningKey, ValidateIssuer = false,
                     ValidateAudience = false, ValidateLifetime = true))
    |> ignore

    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

    // Infra
    let sha256 = SHA256.Create()
    let hashProvider = HashProvider sha256
    let sha256Provider = SHA256Provider sha256

    services.AddSingleton<IConfig, Config>() |> ignore
    services.AddSingleton<IPermissionsFromRoles, PermissionsFromRoles>() |> ignore
    services.AddSingleton<IHashProvider>(hashProvider) |> ignore
    services.AddSingleton<ISHA256Provider>(sha256Provider) |> ignore
    services.AddSingleton<IPasswordSaltProvider, PasswordSaltProvider>() |> ignore

    // Configure DataContext
    let connectionString = context.Configuration.GetConnectionString "PostgreSQL"
    services.AddDbContext<DbDataContext>
        ((fun o ->
        let o' = o.EnableSensitiveDataLogging true
        NpgsqlDbContextOptionsExtensions.UseNpgsql
            (o', connectionString, (fun b -> b.MigrationsAssembly("PRR.Data.DataContextMigrations") |> ignore))
        |> ignore))
    |> ignore


    printf "+++%s %O" context.HostingEnvironment.EnvironmentName
    // Actors system

    // TODO : Why not working like so https://stackoverflow.com/questions/56442871/is-there-a-way-to-use-f-record-types-to-extract-the-appsettings-json-configurat

    let mailEnv: MailEnv =
        { FromEmail = context.Configuration.GetValue("MailSender:FromEmail")
          FromName = context.Configuration.GetValue("MailSender:FromName")
          Project =
              { Name = context.Configuration.GetValue("MailSender:Project:Name")
                BaseUrl = context.Configuration.GetValue("MailSender:Project:BaseUrl")
                ConfirmSignUpUrl = context.Configuration.GetValue("MailSender:Project:ConfirmSignUpUrl")
                ResetPasswordUrl = context.Configuration.GetValue("MailSender:Project:ResetPasswordUrl") } }

    let sendGridApiKey = context.Configuration.GetValue("SendGridApiKey")
    let mailSender = PRR.API.Infra.Mail.SendGridMail.createSendMail sendGridApiKey

    let systemEnv: SystemEnv =
        { SendMail = createSendMail mailEnv mailSender
          GetDataContextProvider =
              fun () -> new DataContextProvider(services.BuildServiceProvider().CreateScope()) :> IDataContextProvider
          HashProvider = (hashProvider :> IHashProvider).GetHash
          PasswordSalter = services.BuildServiceProvider().GetService<IPasswordSaltProvider>().SaltPassword
          AuthConfig =
              { IdTokenExpiresIn = config.Jwt.IdTokenExpiresIn
                AccessTokenExpiresIn = config.Jwt.AccessTokenExpiresIn
                RefreshTokenExpiresIn = config.Jwt.RefreshTokenExpiresIn
                SignUpTokenExpiresIn = config.SignUpTokenExpiresIn
                ResetPasswordTokenExpiresIn = config.ResetPasswordTokenExpiresIn }
          EventHandledCallback = fun _ -> () }

#if TEST
    //For tests
    services.AddSingleton<SystemEnv>(fun _ -> systemEnv) |> ignore
#endif

#if E2E
    let sys = setUp' systemEnv "akka.e2e.hocon"
    services.AddSingleton<ICQRSSystem>(fun _ -> sys) |> ignore
#else
    // Tests must initialize sys by themselves
    let sys = setUp systemEnv
    services.AddSingleton<ICQRSSystem>(fun _ -> sys) |> ignore
#endif


let configureLogging (builder: ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error).AddConsole().AddDebug() |> ignore

let configureAppConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
    config.AddJsonFile("appsettings.json", false, true)
          .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName, true)
          .AddEnvironmentVariables() |> ignore



[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        // .UseWebRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .ConfigureAppConfiguration(configureAppConfiguration)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
