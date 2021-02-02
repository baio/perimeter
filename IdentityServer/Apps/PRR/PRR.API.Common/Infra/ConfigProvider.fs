namespace PRR.API.Common.Infra

[<AutoOpen>]
module ConfigProvider = 

    type IConfigProvider<'a> =
        abstract GetConfig: (unit -> 'a)

    type ConfigProvider<'a>(appConfig: 'a) =
        interface IConfigProvider<'a> with
            member __.GetConfig = fun () -> appConfig

