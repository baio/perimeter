namespace Common.Domain.Utils

open Common.Domain.Models
open Common.Utils

[<AutoOpen>]
module UniqueConstraintException =

    let private exceptionMessage = sprintf "23505: duplicate key value violates unique constraint \"%s\""

    let uniqueConstraintException pattern message (input: 'a :> System.Exception) =
        match box input with
        | :? Microsoft.EntityFrameworkCore.DbUpdateException as ex when ex.InnerException.Message =
                                                                            exceptionMessage pattern ->
            message
            |> Conflict
            |> Some
        | _ -> None


    let (|UniqueConstraintException|_|) pattern message (input: 'a :> System.Exception) =
        uniqueConstraintException pattern message input
