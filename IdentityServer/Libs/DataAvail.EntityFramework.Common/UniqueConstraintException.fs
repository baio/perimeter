namespace DataAvail.EntityFramework.Common

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

    let updateNotFoundException (input: 'a :> System.Exception) =
        match box input with
        | :? Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException as ex when ex.HResult = -2146233088 ->
            NotFound |> Some
        | _ -> None

    let (|UniqueConstraintException|_|) pattern message (input: 'a :> System.Exception) =
        uniqueConstraintException pattern message input

    let (|UpdateNotFoundException|_|) (input: 'a :> System.Exception) =
        updateNotFoundException input
