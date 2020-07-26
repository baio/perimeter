namespace PRR.API.Routes.Tenant
open Common.Domain.Giraffe
open PRR.API.Routes.DIHelpers

[<AutoOpen>]
module private Utils = 

    let wrapAudienceGuard' f = wrapTask f audienceGuard

    let wrapGetAudience f i = getDataContext >> f i

    let wrapAudienceGuard f id =
        wrapAudienceGuard' (wrapGetAudience f id)
