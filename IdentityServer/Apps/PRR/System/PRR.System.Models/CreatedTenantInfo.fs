namespace PRR.System.Models

[<AutoOpen>]
module CreatedTenantInfo =
    type CreatedTenantInfo = {
        TenantId: int
        TenantManagementDomainId: int
        TenantManagementApiId: int
        TenantManagementApplicationId: int        
        TenantManagementApiIdentifier: string
        TenantManagementApplicationClientId: string
        DomainPoolId: int
        DomainId: int
        DomainManagementApplicationId: int
        DomainManagementApplicationClientId: string
        DomainManagementApiId: int
        DomainManagementApiIdentifier: string
        SampleApiId: int
        SampleApiIdentifier: string
        SampleApplicationId: int
        SampleApplicationClientId: string        
    }    

