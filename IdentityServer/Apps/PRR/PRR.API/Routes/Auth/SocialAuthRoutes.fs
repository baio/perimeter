namespace PRR.API.Routes.Authx.SocialAuthRoutes

open Common.Domain.Models.Models

module SocialAuthRoutes =
    
    type SocialType = 
        | Twitter    
        
    type Data = {
        ClientId: string
        Type: SocialType
    }
    
    type Env = {
        HashProvider: HashProvider        
    }
    
    let private handleSocialAuth (env: Env) (data: Data) =
        let token = env.HashProvider()
        ()
        

