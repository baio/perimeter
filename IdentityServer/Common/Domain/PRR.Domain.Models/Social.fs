namespace Common.Domain.Models

[<AutoOpen>]
module Social =

    type SocialType =
        | Github
        | Twitter
        | Google

    type Social = { Id: string; Type: SocialType }

    let socialType2Name socialType =
        match socialType with
        | Github -> "github"
        | Twitter -> "twitter"
        | Google -> "google"

    let socialName2Type socialName =
        match socialName with
        | "github" -> SocialType.Github
        | "twitter" -> SocialType.Twitter
        | "google" -> SocialType.Google
        | _ ->
            raise
                (sprintf "Social [%s] is not found" socialName
                 |> exn)
