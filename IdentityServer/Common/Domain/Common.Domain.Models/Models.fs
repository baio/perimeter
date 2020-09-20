namespace Common.Domain.Models

open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type TenantId = int

    type DomainPoolId = int

    type DomainId = int

    type ClientId = string

    type Issuer = string

    type RoleId = int

    type Scope = string

    type Uri = string

    type UserId = int

    type CompanyId = int

    type Token = string

    type Email = string

    type Sub = string

    type StringSalter = string -> string

    type HashProvider = unit -> string

    type Sha256Provider = string -> string

    [<Measure>]
    type days

    [<Measure>]
    type hours

    [<Measure>]
    type minutes

    [<Measure>]
    type seconds

    [<Measure>]
    type milliseconds

    type AuthStringsProvider =
        { ClientId: unit -> string
          ClientSecret: unit -> string
          AuthorizationCode: unit -> string
          HS256SigningSecret: unit -> string
          RS256XMLParams: unit -> string }

    type AudienceScopes =
        { Audience: string
          Scopes: string seq }
