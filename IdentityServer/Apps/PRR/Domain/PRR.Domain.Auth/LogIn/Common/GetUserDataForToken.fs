namespace PRR.Domain.Auth.LogIn.Common

[<AutoOpen>]
module GetUserDataForToken =

    open PRR.Data.DataContext
    open PRR.Data.Entities
    open PRR.Domain.Models
    open DataAvail.EntityFramework.Common
    open DataAvail.Common    

    [<CLIMutable>]
    type TokenData =
        { Id: int
          FirstName: string
          LastName: string
          Email: string
          SocialType: SocialType option }


    let private getUserDataForTokenNotSocial (dataContext: DbDataContext) filterUser =
        query {
            for user in dataContext.Users do
                where ((%filterUser) user)

                select
                    { Id = user.Id
                      FirstName = user.FirstName
                      LastName = user.LastName
                      Email = user.Email
                      SocialType = None }
        }
        |> toSingleOptionAsync

    let private getUserDataForTokenSocial (dataContext: DbDataContext) filterUser socialType =
        let socialTypeName = socialType2Name socialType

        query {
            for si in dataContext.SocialIdentities do
                where
                    (((%filterUser) si.User)
                     && si.SocialName = socialTypeName)

                select (si.Name, si.Email, si.UserId)
        }
        |> toSingleOptionAsync
        |> TaskUtils.map
            (Option.map (fun (name, email, userId) ->
                let (firstName, lastName) = splitName name

                { Id = userId
                  FirstName = firstName
                  LastName = lastName
                  Email = email
                  SocialType = Some socialType }: TokenData))

    let getUserDataForToken' (dataContext: DbDataContext) filterUser socialType =
        match socialType with
        | Some socialType -> getUserDataForTokenSocial dataContext filterUser socialType
        | None -> getUserDataForTokenNotSocial dataContext filterUser

    let getUserDataForToken (dataContext: DbDataContext) userId =
        getUserDataForToken' dataContext <@ fun (user: User) -> (user.Id = userId) @>
