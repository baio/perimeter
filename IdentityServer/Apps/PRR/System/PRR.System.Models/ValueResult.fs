namespace PRR.System.Models

[<AutoOpen>]
module Common =
    
    /// Akka tell with options doesn't work since options represented internally by null
    /// And akka tell doesn't accept nulls 
    type ValueResult<'a> =
        | Value of 'a
        | Empty        
    
    let valueResultFromOption =
        function
        | Some x -> ValueResult.Value x
        | None -> ValueResult.Empty

    let optionFromValueResult =
        function
        | ValueResult.Value x -> Some x
        | ValueResult.Empty -> None
        

    
    

