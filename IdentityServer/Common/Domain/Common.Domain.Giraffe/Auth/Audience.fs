namespace Common.Domain.Giraffe

open Common.Domain.Models.Exceptions
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe

[<AutoOpen>]
module Audience =
    let validateAudience aud =
        bindUserClaimsAudiences
        >> Seq.tryFind (fun x -> x = aud)
        >> Options.noneFails Forbidden


    let audienceGuard aud: HttpHandler =
        fun next -> (validateAudience aud |> ofReader) >>= (fun _ ctx -> next ctx)
