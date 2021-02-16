namespace NamelessInteractive.FSharp.MongoDB


module SetUp =

    let registerSerializationAndConventions () =
        SerializationProviderModule.Register()
        Conventions.ConventionsModule.Register()
