namespace DataAvail.Giraffe.Common

[<AutoOpen>]
module Audience =
    
    open Giraffe
    open DataAvail.Common
    open DataAvail.Common.ReaderTask
    open DataAvail.Http.Exceptions
    
    let validateAudience aud =
        bindUserClaimsAudiences
        >> Seq.tryFind (fun x -> x = aud)
        >> Option.noneFails Forbidden'


    let audienceGuard aud: HttpHandler =
        fun next -> (validateAudience aud |> ofReader) >>= (fun _ ctx -> next ctx)
