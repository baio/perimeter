﻿namespace Common.Domain.Models

[<AutoOpen>]
module Exceptions =

    exception NotFound

    exception UnAuthorized of string option

    let UnAuthorized' = UnAuthorized None

    let unAuthorized = Some >> UnAuthorized

    exception Forbidden of string option

    let Forbidden' = Forbidden None

    let forbidden = Some >> Forbidden

    exception Conflict of ConflictError

    exception Unexpected of string option

    let unexpected = Some >> Unexpected

    let Unexpected' = Unexpected None

    exception BadRequest of BadRequestError array
