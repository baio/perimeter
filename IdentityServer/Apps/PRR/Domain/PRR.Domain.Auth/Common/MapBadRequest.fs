namespace PRR.Domain.Auth.Common

open DataAvail.Http.Exceptions

[<AutoOpen>]
module MapBadRequest =
    let mapBadRequest (errors: BadRequestError option array) =
        errors
        |> Array.choose id
        |> function
        | [||] -> None
        | errors -> Some(BadRequest errors)
