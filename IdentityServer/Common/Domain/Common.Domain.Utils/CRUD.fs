namespace Common.Domain.Utils

open Common.Domain.Models.Exceptions
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.EntityFrameworkCore
open System.Threading.Tasks

module CRUD =

    let private saveCRUDChangeAsync (dbContext: DbContext) =
        task {
            try
                do! saveChangesAsync dbContext
            with :? Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException as ex ->
                if ex.Message.Contains "Database operation expected to affect 1 row(s) but actually affected 0 row(s)" then
                    raise NotFound
                else raise ex
        }

    type Create<'dto, 'r, 'dbContext when 'dbContext :> DbContext> = 'dto -> 'dbContext -> Task<'r>

    let createCatch<'b, 'a, 'c, 'dbContext when 'b: not struct and 'dbContext :> DbContext> exceptionHandler
        (dto2enty: 'a -> 'b) (resMap: 'b -> 'c): Create<'a, 'c, 'dbContext> =
        fun dto dbContext ->
            let entity = (dto2enty dto) |> (add' dbContext)
            task {
                try
                    do! saveChangesAsync dbContext
                with ex ->
                    return exceptionHandler ex
                let res = resMap entity
                return res
            }

    let create<'b, 'a, 'c, 'dbContext when 'b: not struct and 'dbContext :> DbContext> =
        createCatch<'b, 'a, 'c, 'dbContext> (fun ex -> raise ex)

    let validateCreateCatch<'b, 'a, 'c, 'dbContext when 'b: not struct and 'dbContext :> DbContext> exceptionHandler
        (validate: 'a -> 'dbContext -> Task<unit>) (dto2enty: 'a -> 'b) (resMap: 'b -> 'c): Create<'a, 'c, 'dbContext> =
        fun x dbContext ->
            task {
                do! validate x dbContext
                return! createCatch exceptionHandler dto2enty resMap x dbContext
            }

    let validateCreate<'b, 'a, 'c, 'dbContext when 'b: not struct and 'dbContext :> DbContext> =
        validateCreateCatch<'b, 'a, 'c, 'dbContext> (fun ex -> raise ex)

    type Update<'id, 'dto, 'dbContext when 'dbContext :> DbContext> = 'id * 'dto -> ('dbContext -> Task<unit>)

    let updateTask<'b, 'id, 'a, 'dbContext when 'b: not struct and 'dbContext :> DbContext> (dto2enty: 'id -> 'b)
        (mapEntity: 'dbContext -> 'a -> 'b -> Task<unit>): Update<'id, 'a, 'dbContext> =
        fun (id, dto) dbContext ->
            task {
                do! update' dbContext (fun x -> mapEntity dbContext dto x) (dto2enty id)
                do! saveCRUDChangeAsync dbContext
            }

    let validateUpdateTask<'b, 'id, 'a, 'dbContext when 'b: not struct and 'dbContext :> DbContext> (validate: 'id * 'a -> ('dbContext -> Task<unit>))
        (dto2enty: 'id -> 'b) (mapEntity: 'dbContext -> 'a -> 'b -> Task<unit>): Update<'id, 'a, 'dbContext> =
        fun x dbContext ->
            task {
                do! validate x dbContext
                return! updateTask dto2enty mapEntity x dbContext
            }

    let updateCatch<'b, 'id, 'a, 'dbContext when 'b: not struct and 'dbContext :> DbContext> exceptionHandler
        (dto2enty: 'id -> 'b) (mapEntity: 'a -> 'b -> unit): Update<'id, 'a, 'dbContext> =
        fun (id, dto) dbContext ->
            let entity = dto2enty id
            update dbContext (mapEntity dto) entity
            task {
                try
                    do! saveChangesAsync dbContext
                with ex ->
                    return exceptionHandler ex
            }

    let update<'b, 'id, 'a, 'dbContext when 'b: not struct and 'dbContext :> DbContext> =
        updateCatch<'b, 'id, 'a, 'dbContext> (function
            | UpdateNotFoundException ex ->
                raise ex
            | ex ->
                raise ex)

    let validateUpdate<'b, 'id, 'a, 'dbContext when 'b: not struct and 'dbContext :> DbContext> (validate: 'id * 'a -> ('dbContext -> Task<unit>))
        (dto2enty: 'id -> 'b) (mapEntity: 'a -> 'b -> unit): Update<'id, 'a, 'dbContext> =
        fun x dbContext ->
            task {
                do! validate x dbContext
                return! update dto2enty mapEntity x dbContext
            }

    let validateUpdateCatch<'b, 'id, 'a, 'dbContext when 'b: not struct and 'dbContext :> DbContext> exceptionHandler
        (validate: 'id * 'a -> ('dbContext -> Task<unit>)) (dto2enty: 'id -> 'b) (mapEntity: 'a -> 'b -> unit): Update<'id, 'a, 'dbContext> =
        fun x dbContext ->
            task {
                do! validate x dbContext
                return! updateCatch exceptionHandler dto2enty mapEntity x dbContext
            }

    type Remove<'id, 'dbContext when 'dbContext :> DbContext> = 'id -> 'dbContext -> Task<unit>

    let remove<'b, 'id, 'dbContext when 'b: not struct and 'dbContext :> DbContext> (dto2entity: 'id -> 'b): Remove<'id, 'dbContext> =
        fun id dbContext ->
            let entity = dto2entity id
            remove dbContext entity
            saveCRUDChangeAsync dbContext

    type GetOne<'id, 'r, 'dbContext when 'dbContext :> DbContext> = 'id -> 'dbContext -> Task<'r>

    let getOneOption<'b, 'id, 'c, 'd, 'dbContext when 'b: not struct and 'dbContext :> DbContext> (f: 'c option -> 'd)
        (whr: Quotations.Expr<'b -> 'id -> bool>) (sel: Quotations.Expr<'b -> 'c>) (id: 'id) (dbContext: 'dbContext) =
        let dbSet = dbContext.Set<'b>()
        query {
            for x in dbSet do
                where ((%whr) x id)
                select ((%sel) x)
        }
        |> toSingleOptionAsync
        |> TaskUtils.map f

    let getOneWithNoneFail<'b, 'id, 'c, 'dbContext when 'b: not struct and 'dbContext :> DbContext> ex =
        getOneOption<'b, 'id, 'c, 'c, 'dbContext> (function
            | Some x -> x
            | None -> raise ex)

    let getOne<'b, 'id, 'c, 'dbContext when 'b: not struct and 'dbContext :> DbContext> =
        getOneWithNoneFail<'b, 'id, 'c, 'dbContext> NotFound
