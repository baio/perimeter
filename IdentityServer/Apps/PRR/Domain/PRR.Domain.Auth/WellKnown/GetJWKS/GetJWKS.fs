namespace PRR.Domain.Auth.WellKnown.GetJWKS

open PRR.Data.Entities


[<AutoOpen>]
module GetJWKS =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.Logging
    open DataAvail.EntityFramework.Common
    open Microsoft.IdentityModel.Tokens
    open PRR.Domain.Auth.Common.Security

    let getJWKS: GetJWKS =
        fun env data ->

            let logger = env.Logger

            logger.LogDebug("Get JWKS with data {@data}", data)

            task {
                let! domainData =
                    query {
                        for domain in env.DataContext.Domains do
                            where
                                (domain.Pool.Tenant.Name = data.Tenant
                                 && domain.Pool.Identifier = data.Domain
                                 && domain.EnvName = data.Env)

                            select (domain.SigningAlgorithm, domain.HS256SigningSecret, domain.RS256Params)
                    }
                    |> toSingleAsync

                logger.LogDebug("JWKS result for domain {@result}", domainData)

                let (signingAlgorithm, hS256SigningSecret, rS256Params) = domainData

                // TODO : Support both
                let key =
                    if signingAlgorithm = SigningAlgorithmType.RS256 then
                        createRS256Key rS256Params
                        |> JsonWebKeyConverter.ConvertFromRSASecurityKey
                    else
                        createHS256Key hS256SigningSecret
                        |> JsonWebKeyConverter.ConvertFromSymmetricSecurityKey

                logger.LogDebug("security key found {@key} {str}", key)
                
                let result =
                    { Alg = key.Alg
                      E = key.E
                      Kid = key.Kid
                      Kty = key.Kty
                      N = key.N
                      Use = key.Use
                      X5c = key.X5c
                      X5t = key.X5t }

                logger.LogDebug("Get jwks success with result {@result}", result)

                return seq { result }
            }
