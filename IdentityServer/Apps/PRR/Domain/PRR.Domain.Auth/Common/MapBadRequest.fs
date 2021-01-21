namespace PRR.Domain.Auth.Common

open Common.Domain.Models.BadRequestErrors
open Common.Domain.Models.Exceptions

[<AutoOpen>]
module MapBadRequest =
    let mapBadRequest (errors: BadRequestError option array) =
        errors
        |> Array.choose id
        |> function
        | [||] -> None
        | errors -> Some(BadRequest errors)
