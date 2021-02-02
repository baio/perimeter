namespace DataAvail.Test.Common

open PRR.Domain.Models
open Microsoft.Extensions.DependencyInjection
open System

[<AutoOpen>]
module ClientFixture =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.AspNetCore.Hosting
    open Microsoft.AspNetCore.TestHost

    type ClientFixture(fn: unit -> IWebHostBuilder, fn2: unit -> IWebHostBuilder) =

        let Host1 = new Host(fn)
        let Host2 = new Host(fn2)

        member __.Server1 = Host1
        member __.Server2 = Host2

        interface IDisposable with
            member __.Dispose() =
                (Host1 :> IDisposable).Dispose()
                (Host2 :> IDisposable).Dispose()
