namespace PRR.Domain.Auth.Common

open System
open PRR.Domain.Models
open DataAvail.KeyValueStorage.Core

[<AutoOpen>]
module KeyValueModels =

    [<PartitionName("SignUp")>]
    type SignUpKV =
        { FirstName: string
          LastName: string
          Email: string
          Password: string
          Token: string
          ExpiredAt: DateTime
          QueryString: string option }


    [<PartitionName("ResetPassword")>]
    type ResetPasswordKV = { Email: string }


    [<PartitionName("LogIn")>]
    type LogInKV =
        { Code: string
          UserId: int
          Social: Social option
          RequestedScopes: string seq
          ValidatedScopes: AudienceScopes seq
          RedirectUri: string
          ClientId: ClientId
          Issuer: string
          CodeChallenge: Token
          ExpiresAt: DateTime }

    [<PartitionName("SSO")>]
    type SSOKV =
        { Code: Token
          TenantId: TenantId
          UserId: UserId
          Email: string
          Social: Social option
          ExpiresAt: DateTime }


    [<PartitionName("SocialLogIn")>]
    type SocialLoginKV =
        { Token: Token
          SocialClientId: string
          Type: SocialType
          ResponseType: string
          State: string
          RedirectUri: Uri
          Scope: Scope
          CodeChallenge: string
          CodeChallengeMethod: string
          DomainClientId: ClientId }

    [<PartitionName("RefreshToken")>]
    type RefreshTokenKV =
        { Token: Token
          ClientId: ClientId
          UserId: UserId
          ExpiresAt: DateTime
          Scopes: string seq
          IsPerimeterClient: bool
          SocialType: SocialType option }
