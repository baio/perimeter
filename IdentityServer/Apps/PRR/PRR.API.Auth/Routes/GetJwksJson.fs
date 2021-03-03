namespace PRR.API.Auth.Routes

module GetJwksJson =

    open Giraffe

    let handler issuerPath next ctx = json {| Ok = true |} next ctx
