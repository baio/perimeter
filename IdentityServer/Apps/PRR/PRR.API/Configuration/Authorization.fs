namespace PRR.API.Configuration

open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens
open PRR.Domain.Auth

[<AutoOpen>]
module private Authorization =

    let configureAuthorization (config: JwtConfig) (services: IServiceCollection) =

        // Authentication
        let issuerSigningKey =
            config.AccessTokenSecret
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
