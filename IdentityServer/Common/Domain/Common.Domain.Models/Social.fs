namespace Common.Domain.Models

[<AutoOpen>]
module Social =

    type SocialType = | Github

    type Social = { Id: string; Type: SocialType }

    let socialType2Name socialType =
        match socialType with
        | Github -> "github"

    let socialName2Type socialName =
        match socialName with
        | "github" -> SocialType.Github
        | _ ->
            raise
                (sprintf "Social [%s] is not found" socialName
                 |> exn)
