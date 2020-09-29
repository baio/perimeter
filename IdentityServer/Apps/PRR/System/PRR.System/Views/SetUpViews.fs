namespace PRR.System.Views
open PRR.System.Views.LogInView

[<AutoOpen>]
module SetUpViews =

    let setUpViews sys aref connectionString =
        let db = setUpViewDb connectionString
        initLoginView db sys aref
