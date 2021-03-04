namespace PRR.Domain.Auth.WellKnown.GetJWKS

[<AutoOpen>]
module Models =

    open System.Threading.Tasks
    open Microsoft.Extensions.Logging
    open PRR.Data.DataContext
    
    type Env =
        { DataContext: DbDataContext
          Logger: ILogger }

    type Data =
        { Tenant: string
          Domain: string
          Env: string }
        
    type JWKKey = {
        Alg: string
        E: string
        Kid: string
        Kty: string
        N: string
        Use: string
        X5c: string seq
        X5t: string
    }        

    type GetJWKS = Env -> Data -> Task<JWKKey seq>