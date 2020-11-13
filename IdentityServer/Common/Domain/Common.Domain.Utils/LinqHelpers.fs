namespace Common.Domain.Utils

open Common.Domain.Models.Exceptions
open System.ComponentModel.DataAnnotations.Schema

[<AutoOpen>]
module LinqHelpers =

    open Common.Domain.Models
    open Common.Utils
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.EntityFrameworkCore
    open Microsoft.EntityFrameworkCore.Infrastructure
    open Microsoft.EntityFrameworkCore.Metadata.Internal
    open System.Linq

    let toListAsyncTraceException q =
        try
            let result =
                EntityFrameworkQueryableExtensions.ToListAsync(q)

            result
        with ex ->
            printf "%O" ex
            raise ex

    let toListAsync =
        EntityFrameworkQueryableExtensions.ToListAsync

    let toCountAsync =
        EntityFrameworkQueryableExtensions.CountAsync

    let notFoundRaiseError ex =
        EntityFrameworkQueryableExtensions.CountAsync
        >> map (fun cnt -> if cnt = 0 then raise ex)

    let toListAsync' x = x |> toListAsync |> map (List.ofSeq)

    let toSeqAsync x =
        x
        |> toListAsync
        |> map (fun y ->
            seq {
                for z in y do
                    yield z
            })

    let toSingleOptionAsync (q: IQueryable<_>) =
        task {
            try
                let result = q.Single()
                return Some result
            with ex ->
                printf "toSingleOptionAsync: %O" ex
                return None
        }

    let toSingleExnAsync ex (q: IQueryable<_>) =
        task {
            match! toSingleOptionAsync q with
            | Some r -> return r
            | None -> return raise ex
        }

    let toSingleAsync x = x |> toSingleExnAsync NotFound

    let setUnchanged (dataContext: DbContext) x =
        dataContext.Entry(x).State <- EntityState.Unchanged
        x

    let toSingleUnchangedAsync dataContext x =
        (setUnchanged dataContext) <!> toSingleAsync x


    let countAsync =
        EntityFrameworkQueryableExtensions.CountAsync

    let select x (q: IQueryable<'a>) =
        query {
            for i in q do
                select ((%x) i)
        }

    let add' (dbContext: DbContext) =
        dbContext.Set<'a>().Add >> (fun x -> x.Entity)

    let add (dbContext: DbContext) = add' dbContext >> ignore

    let update'<'a, 'b when 'a: not struct> (dbContext: DbContext) (fn: 'a -> 'b) (x: 'a) =
        let e = dbContext.Set<'a>().Attach x
        fn e.Entity

    let update (dbContext: DbContext) fn (x: 'a) = update' dbContext fn x |> ignore

    let updateRange (dbContext: DbContext) fn (x: 'a seq) = x |> Seq.iter (update dbContext fn)

    let addRange (dbContext: DbContext) (x: 'a seq) =
        x |> dbContext.Set<'a>().AddRange |> ignore

    let attachRange (dbContext: DbContext) (x: 'a seq) =
        x |> dbContext.Set<'a>().AttachRange |> ignore

    let remove (dbContext: DbContext) (x: 'a) =
        x |> dbContext.Set<'a>().Remove |> ignore

    let removeRange (dbContext: DbContext) (x: 'a seq) =
        x |> dbContext.Set<'a>().RemoveRange |> ignore

    let saveChangesAsync (dbContext: DbContext) =
        dbContext.SaveChangesAsync() |> ignoreTask

    let addRemoveRange (dbContext: DbContext) (a: 'a seq, (r: 'a seq)) =
        a |> addRange dbContext
        r |> removeRange dbContext
        ()

    (**
        Returns tuple with sequences
        1st incoming without current
        2nd current without incoming
    *)

    let splitAddRemove incoming current =
        (Seq.except current incoming), (Seq.except incoming current)

    let splitAddRemoveRange dbContext incoming =
        splitAddRemove incoming
        >> addRemoveRange dbContext

    let querySplitAddRemoveRange dbContext incoming =
        toListAsync
        >> map (splitAddRemoveRange dbContext incoming)

    (**
        Returns tuple with sequences
        1st incoming without current
        2nd current and incoming
    *)

    let splitAddUpdate incoming current =
        (Seq.except current incoming), (incoming.Intersect current)

    let groupByAsync''' (fn: System.Tuple<_, _> -> _) x = x |> toListAsync |> map (Seq.groupBy fn)

    let groupByAsync'' x = x |> groupByAsync''' fst

    let groupByAsync' x =
        x
        |> groupByAsync''
        |> map (Seq.map (fun (k, v) -> (k, v |> Seq.map snd)))

    let groupByAsync fn = groupByAsync' >> map (Seq.map fn)

    //

    let getTableName' (context: DbContext) (t: System.Type) =
        let entityType = context.Model.FindEntityType(t)
        entityType.GetTableName()

    let getTableName<'t> (context: DbContext) =
        let entityType = typedefof<'t>
        getTableName' context entityType

    let getDbSetTableName (dbSet: DbSet<'t>) =
        let context =
            dbSet.GetService<ICurrentDbContext>().Context

        let tableName = getTableName<'t> (context)
        (context, tableName)

    let unzipProps x =
        let t = x.GetType()
        let p = t.GetProperties()
        p
        |> Array.map (fun m -> (m.Name, m.GetValue(x)))
        |> Array.unzip

    let stringCommand i s =
        Seq.indexed
        >> Seq.map (fun (i', x) -> sprintf "\"%s\"={%i}" x (i' + i))
        >> String.concat s

    let prepareUpdateRawAsync (dbSet: DbSet<_>) set where =
        // TODO: Check fields
        let (whereKeys, whereValues) = unzipProps where
        let (setKeys, setValues) = unzipProps set
        let setCommand = stringCommand 0 "," setKeys

        let whereCommand =
            stringCommand setKeys.Length " AND " whereKeys

        let (context, tableName) = getDbSetTableName dbSet

        let command =
            sprintf "UPDATE \"%s\" SET %s WHERE %s" tableName setCommand whereCommand

        let prms = setValues.Concat whereValues
        (context, command, prms)

    let updateRawAsync (dbSet: DbSet<'t>) set wher =
        let (context, command, prms) = prepareUpdateRawAsync dbSet set wher
        context.Database.ExecuteSqlRawAsync(command, prms)

    let updateSingleRawAsyncExn ex (dbSet: DbSet<'t>) set where =
        task {
            let! res = updateRawAsync dbSet set where
            if res = 0 then raise ex
        }

    let updateSingleRawAsync x = x |> updateSingleRawAsyncExn NotFound

    let prepareRemoveSingleRawAsync (dbSet: DbSet<_>) where =
        let (whereKeys, whereValues) = unzipProps where
        let whereCommand = stringCommand 0 " AND " whereKeys
        let (context, tableName) = getDbSetTableName dbSet

        let command =
            sprintf "DELETE FROM \"%s\" WHERE %s" tableName whereCommand

        let prms = whereValues
        (context, command, prms)

    let removeRawAsync (dbSet: DbSet<'t>) where =
        let (context, command, prms) = prepareRemoveSingleRawAsync dbSet where
        context.Database.ExecuteSqlRawAsync(command, prms)

    let removeSingleRawAsync (dbSet: DbSet<'t>) where =
        task {
            let! res = removeRawAsync dbSet where
            if res = 0 then return! raiseTask (NotFound)
        }

    let createOrUpdateRawAsync (dbSet: DbSet<_>) conflictKeys set =
        let (context, tableName) = getDbSetTableName (dbSet)
        let (setKeys, setValues) = unzipProps set

        let keys =
            setKeys
            |> Seq.map (sprintf "\"%s\"")
            |> String.concat ","

        let keyValues =
            setValues
            |> Seq.indexed
            |> Seq.map (fun (i, _) -> sprintf "{%i}" i)
            |> String.concat ","

        let setKeys = stringCommand 0 "," setKeys

        let conflictKeys =
            conflictKeys
            |> Seq.map (sprintf "\"%s\"")
            |> String.concat ","

        let command =
            sprintf
                "INSERT INTO \"%s\" (%s) VALUES (%s) ON CONFLICT (%s) DO UPDATE SET %s"
                tableName
                keys
                keyValues
                conflictKeys
                setKeys

        context.Database.ExecuteSqlRawAsync(command, setValues)


    let queryRawAsync<'a> (context: DbContext) (mp: obj -> 'a) q =
        use command =
            context.Database.GetDbConnection().CreateCommand()

        command.CommandText <- q
        task {
            use! result = command.ExecuteReaderAsync()
            let mutable f = true

            let list =
                System.Collections.Generic.List<obj []>()

            while f do
                let! f' = result.ReadAsync()
                f <- f'

                if f then
                    let arr = Array.create result.FieldCount null
                    result.GetValues arr |> ignore
                    list.Add arr

            return list |> Seq.map mp
        }
