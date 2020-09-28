namespace PRR.System.Views

[<AutoOpen>]
module SetUpViews =
    
    let setUpViews sys aref connectionString  =        
        let db = setUpViewDb connectionString
        initLoginView db sys aref 

