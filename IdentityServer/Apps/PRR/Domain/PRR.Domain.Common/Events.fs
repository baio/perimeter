namespace PRR.Domain.Common

open System
open Common.Domain.Models

module Events =

    type LogIn =
        { Social: Social option
          DateTime: DateTime
          UserId: int
          ClientId: string }
