module PRR.API.App

open Akka.Actor
open Akka.Configuration
open Akkling
open Giraffe
open Giraffe.Serialization
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Serialization
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
    subRoute
        "/api"
        (choose [ Auth.createRoutes ()
                  Me.createRoutes ()
                  Tenant.createRoutes ()
                  PingRoutes.createRoutes ()
                  ApplicationInfo.createRoutes ()
#if E2E
                  E2E.createRoutes ()
#endif
                  setStatusCode 404 >=> text "Not Found" ])


let migrateDatabase (webHost: IWebHost) =
    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/april/data-points-ef-core-in-a-docker-containerized-app#migrating-the-database
    use scope = webHost.Services.CreateScope()
    let services = scope.ServiceProvider
    try
        let db =
            services.GetRequiredService<DbDataContext>()

        db.Database.Migrate()
    with ex -> printfn "An error occurred while migrating the database. %O" ex


let createDbContext (connectionString: string) =
    let optionsBuilder = DbContextOptionsBuilder<DbDataContext>()
    optionsBuilder.UseNpgsql
        (connectionString,
         (fun b ->
             b.MigrationsAssembly("PRR.Data.DataContextMigrations")
             |> ignore))
    DbDataContext(optionsBuilder.Options)

let configureCors (builder: CorsPolicyBuilder) =
    // builder.WithOrigins([| "http://localhost:4200" |]).AllowAnyMethod().AllowAnyHeader().WithHeaders([|"Access-Control-Allow-Credentials"|]) |> ignore
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env =
        app.ApplicationServices.GetService<IHostingEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     //| false -> app.UseGiraffeErrorHandler errorHandler).UseHttpsRedirection().UseAuthentication().UseAuthorization()
     | false -> app.UseGiraffeErrorHandler errorHandler).UseAuthentication().UseAuthorization().UseCors(configureCors)
        .UseGiraffe(webApp)

let configureServices (context: WebHostBuilderContext) (services: IServiceCollection) =
    // Json
    (*
    let customSettings =
        JsonSerializerSettings(ContractResolver = CamelCasePropertyNamesContractResolver())
    customSettings.Converters.Add(StringEnumConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(customSettings))
    |> ignore
    *)

    // auth
    let config =
        Infra.Config.getConfig context.Configuration ()
    // Authentication
    let issuerSigningKey =
        config.Jwt.AccessTokenSecret
        |> System.Text.Encoding.ASCII.GetBytes
        |> SymmetricSecurityKey

    services.AddAuthorization() |> ignore
    services.AddAuthentication(fun options ->
            // https://stackoverflow.com/questions/45763149/asp-net-core-jwt-in-uri-query-parameter/53295042#53295042
            options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun x ->
            x.RequireHttpsMetadata <- false
            x.SaveToken <- true
            x.TokenValidationParameters <-
                TokenValidationParameters
                    (ValidateIssuerSigningKey = true,
                     IssuerSigningKey = issuerSigningKey,
                     ValidateIssuer = false,
                     ValidateAudience = false,
#if E2E
                     ValidateLifetime = false
#else
                     ValidateLifetime = true
#endif
                    ))
    |> ignore

    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

    // Infra
    let sha256 = SHA256.Create()
    let hashProvider = HashProvider sha256
    let sha256Provider = SHA256Provider sha256

    services.AddSingleton<IConfig, Config>() |> ignore
    services.AddSingleton<IPermissionsFromRoles, PermissionsFromRoles>()
    |> ignore
    services.AddSingleton<IHashProvider>(hashProvider)
    |> ignore
    services.AddSingleton<ISHA256Provider>(sha256Provider)
    |> ignore
    services.AddSingleton<IPasswordSaltProvider, PasswordSaltProvider>()
    |> ignore
    services.AddSingleton<IAuthStringsProvider, AuthStringsProvider>()
    |> ignore

    // Configure DataContext
    let loggerFactory =
        LoggerFactory.Create(fun builder -> (*builder.AddConsole() |> ignore *) ())

    let connectionString =
        context.Configuration.GetConnectionString "PostgreSQL"

    printfn "ENV %s" context.HostingEnvironment.EnvironmentName
    printfn "Connection string: %s" connectionString

    services.AddDbContext<DbDataContext>
        ((fun o ->
            let o' =
                o.UseLoggerFactory(loggerFactory).EnableSensitiveDataLogging true

            NpgsqlDbContextOptionsExtensions.UseNpgsql
                (o',
                 connectionString,
                 (fun b ->
                     b.MigrationsAssembly("PRR.Data.DataContextMigrations")
                     |> ignore))
            |> ignore))
    |> ignore

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

    let sendGridApiKey =
        context.Configuration.GetValue("SendGridApiKey")

    let mailSender =
        PRR.API.Infra.Mail.SendGridMail.createSendMail sendGridApiKey

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


#if TEST
    // Tests must initialize sys by themselves
    //For tests
    services.AddSingleton<SystemEnv>(fun _ -> systemEnv)
    |> ignore
#else
    let env =
        (context.HostingEnvironment.EnvironmentName.ToLower())

    let akkaConfFile = sprintf "akka.%s.hocon" env
    printfn "Akka conf file %s" akkaConfFile
    let sys = setUp' systemEnv akkaConfFile
    services.AddSingleton<ICQRSSystem>(fun _ -> sys)
    |> ignore
#endif


let configureLogging (builder: ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error).AddConsole().AddDebug()
    |> ignore

let configureAppConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
    let env =
        (context.HostingEnvironment.EnvironmentName.ToLower())

    config.AddJsonFile("appsettings.json", false, true).AddJsonFile(sprintf "appsettings.%s.json" env, true)
          .AddEnvironmentVariables()
    |> ignore

[<EntryPoint>]
let main _ =
    let app =
        WebHostBuilder().UseKestrel()
            // .UseWebRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration().ConfigureAppConfiguration(configureAppConfiguration)
            .Configure(Action<IApplicationBuilder> configureApp).ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging).Build()

    // TODO : Prod migrations ?
    // https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#apply-migrations-at-runtime
#if !TEST
    // test will apply migrations by itself
    migrateDatabase app
#endif
    app.Run()
    0
