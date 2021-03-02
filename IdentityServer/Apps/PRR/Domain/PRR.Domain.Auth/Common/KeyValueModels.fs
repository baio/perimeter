namespace PRR.Domain.Auth.Common

open System
open PRR.Data.Entities
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
          // Case when user was registered through social provider before, just need update password on confirm
          ExistentUserId: int option
          ExpiredAt: DateTime
          RedirectUri: string }


    [<PartitionName("ResetPassword")>]
    type ResetPasswordKV = { Email: string; RedirectUri: string }


    [<PartitionName("LogIn")>]
    type LogInKV =
        { Code: string
          UserId: int
          UserEmail: string
          Social: Social option
          RequestedScopes: string seq
          ValidatedScopes: AudienceScopes seq
          RedirectUri: string
          ClientId: ClientId
          Issuer: string
          CodeChallenge: Token
          ExpiresAt: DateTime
          State: string
          Nonce: string }

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
          Nonce: string
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
