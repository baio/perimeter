namespace Common.Domain.Giraffe 

[<AutoOpen>]
module WrapNoArgs = 

    open System.Threading.Tasks
    open Microsoft.AspNetCore.Http
    open Giraffe
    open FSharp.Control.Tasks.V2.ContextInsensitive

    type HandlerFunNoArgs<'e, 's, 'f> = 'e -> Task<Result<'s, 'f>>

    let wrapperNoArgs (envExtractor: HttpContext -> 'e) (handler: HandlerFunNoArgs<'e, _, _>) (next: HttpFunc)
        (ctx: HttpContext) =
        task {
            let env = envExtractor ctx
            let! result = handler env
            return! match result with
                    | Ok res -> json res next ctx
                    | Error _ -> setStatusCode 500 next ctx
        }

