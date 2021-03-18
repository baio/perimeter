namespace PRR.Domain.Tenant

open PRR.Domain.Models
open PRR.Data.DataContext

[<AutoOpen>]
module Models =

    type IssuerUriData =
        { TenantName: string
          DomainName: string
          EnvName: string }

    type AudienceUriData =
        { IssuerUriData: IssuerUriData
          ApiName: string }

    type IAuthStringsGetter =
        { ClientId: unit -> string
          ClientSecret: unit -> string
          AuthorizationCode: unit -> string
          HS256SigningSecret: unit -> string
          RS256XMLParams: unit -> string
          GetIssuerUri: IssuerUriData -> string
          GetAudienceUri: AudienceUriData -> string }

    type AuthConfig =
        { IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    type Env =
        { DataContext: DbDataContext
          AuthStringsProvider: IAuthStringsGetter
          AuthConfig: AuthConfig }

    type IdOrEntity<'a> =
        | Id of int
        | Entity of 'a

    [<RequireQualifiedAccess>]
    module IdOrEntity =
        let asPair =
            function
            | Id id -> (id, null)
            | Entity ent -> (0, ent)

    type NullableIdOrEntity<'a> =
        | NullableId of int
        | NullableEntity of 'a

    [<RequireQualifiedAccess>]
    module NullableIdOrEntity =
        let asPair =
            function
            | NullableId id -> (System.Nullable id, null)
            | NullableEntity ent -> (System.Nullable(), ent)
