﻿namespace PRR.Domain.Auth.Social.SocialAuth

open System.Threading.Tasks
open Common.Domain.Models
open Microsoft.Extensions.Logging
open PRR.Data.DataContext

[<AutoOpen>]
module Models =

    type internal SocialInfo =
        { ClientId: string
          Attributes: string seq
          Permissions: string seq }

    [<CLIMutable>]
    type Data =
        { Social_Name: string
          Client_Id: ClientId
          Response_Type: string
          State: string
          Redirect_Uri: Uri
          Scope: Scope
          Code_Challenge: string
          Code_Challenge_Method: string }


    // Since there is no app to manage perimeter admin data itself,
    // setup social providers for perimeter runtime through the environment configuration
    type PerimeterSocialClientIds =
        { Github: string
          Google: string
          Twitter: string }

    type OnSuccess = PRR.Sys.Models.Social.Item -> Task<unit>
    type Env =
        { DataContext: DbDataContext
          HashProvider: HashProvider
          SocialCallbackUrl: string
          SocialCallbackExpiresIn: int<milliseconds>
          PerimeterSocialClientIds: PerimeterSocialClientIds
          OnSuccess: OnSuccess
          Logger: ILogger }
