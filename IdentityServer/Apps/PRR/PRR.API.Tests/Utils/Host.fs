namespace DataAvail.Test.Common

open Microsoft.Extensions.DependencyInjection
open System

[<AutoOpen>]
module Host =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.AspNetCore.Hosting
    open Microsoft.AspNetCore.TestHost
    open Xunit.Abstractions

    type Host(fn: unit -> IWebHostBuilder) =

        let mutable _OverrideServices: (IServiceCollection -> unit) option = None
        let mutable _Server: TestServer = null

        member __.OverrideServices(fn: IServiceCollection -> unit) = _OverrideServices <- Some fn

        member __.Server =
            if _Server <> null then
                _Server
            else
                let builder = fn ()

                let builder =
                    match _OverrideServices with
                    | Some f -> builder.ConfigureServices(f)
                    | None -> builder

                _Server <- new TestServer(builder)
                _Server


        member __.GetServer() = __.Server

        member __.HttpGetAsync' x =
            task {
                use client = __.Server.CreateClient()
                return! httpGetAsync' client x
            }

        member __.HttpGetAsync bearer path =
            task {
                use client = __.Server.CreateClient()
                return! httpGetAsync client bearer path
            }


        member __.HttpPostAsync' path payload =
            task {
                use client = __.Server.CreateClient()
                return! httpPostAsync' client path payload
            }

        member __.HttpPostFormAsync' path payload =
            task {
                use client = __.Server.CreateClient()
                return! httpPostFormAsync' client path payload
            }

        member __.HttpPostAsync bearer path payload =
            task {
                use client = __.Server.CreateClient()
                return! httpPostAsync client bearer path payload
            }

        member __.HttpPostFormJsonAsync bearer path payload =
            task {
                use client = __.Server.CreateClient()
                return! httpPostFormJsonAsync client bearer path payload
            }


        member __.HttpPutAsync' path payload =
            task {
                use client = __.Server.CreateClient()
                return! httpPutAsync' client path payload
            }

        member __.HttpPutAsync bearer path payload =
            task {
                use client = __.Server.CreateClient()
                return! httpPutAsync client bearer path payload
            }

        member __.HttpDeleteAsync' path =
            task {
                use client = __.Server.CreateClient()
                return! httpDeleteAsync' client path
            }

        member __.HttpDeleteAsync bearer path =
            task {
                use client = __.Server.CreateClient()
                return! httpDeleteAsync client bearer path
            }

        interface IDisposable with
            member __.Dispose() = __.Server.Dispose()
