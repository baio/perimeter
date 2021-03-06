namespace PRR.Domain.Auth.WellKnown.GetJWKS

[<AutoOpen>]
module GetJWKS =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.Logging
    open DataAvail.EntityFramework.Common
    open Microsoft.IdentityModel.Tokens
    open PRR.Domain.Auth.Common.Security
    open System
    open System.Security.Cryptography
    open PRR.Data.Entities

    let private getPublicKeyFromRS256Params xml =
        let rsa = RSA.Create()
        rsa.FromXmlString xml
        let publicKey = rsa.ExportRSAPublicKey()
        Convert.ToBase64String publicKey

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
                    match signingAlgorithm with
                    | SigningAlgorithmType.RS256 ->
                        let key =
                            createRS256Key rS256Params
                            |> JsonWebKeyConverter.ConvertFromRSASecurityKey

                        logger.LogDebug("key parsed {@key}", key)

                        { Alg = "RS256"
                          E = key.E
                          Kid = key.Kid
                          Kty = key.Kty
                          N = key.N
                          Use = "sig"
                          X5c = key.X5c
                          X5t = key.X5t }

                    | SigningAlgorithmType.HS256 ->
                        let key =
                            createHS256Key hS256SigningSecret
                            |> JsonWebKeyConverter.ConvertFromSymmetricSecurityKey

                        logger.LogDebug("key parsed {@key}", key)
                        
                        { Alg = key.Alg
                          E = key.E
                          Kid = key.Kid
                          Kty = key.Kty
                          N = key.N
                          Use = key.Use
                          X5c = key.X5c
                          X5t = key.X5t }


                logger.LogDebug("security key found {@key}", key)

                return { Keys = seq { key } }
            }
