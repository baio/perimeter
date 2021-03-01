namespace PRR.API.Auth.Routes

module GetJwksJson =

    open Giraffe

    let handler next ctx = json {| Ok = true |} next ctx
