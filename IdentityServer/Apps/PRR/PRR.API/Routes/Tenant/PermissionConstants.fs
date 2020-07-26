namespace PRR.API.Routes.Tenant

[<AutoOpen>]
module private Permissions =

    open PRR.Data.DataContext.Seed
    
    let ARCHIVE_TENANT = Permissions.ArchiveTenant.Name    
    let MANAGE_TENANT_ADMINS = Permissions.ManageTenantAdmins.Name
    let MANAGE_TENANT_DOMAINS = Permissions.ManageTenantDomains.Name
    let ARCHIVE_DOMAIN = Permissions.ArchiveDomain.Name
    let MAMANGE_DOMAIN = Permissions.ManageDomain.Name
    let MANAGE_USERS = Permissions.ManageUsers.Name
    let MANAGE_DOMAIN_ADMINS = Permissions.ManageDomainAdmins.Name    
    let MANAGE_DOMAIN_SUPER_ADMINS = Permissions.ManageDomainSuperAdmins.Name
       
    let READ_PERMISSIONS = Permissions.ReadPermissions.Name
    let MANAGE_PERMISSIONS = Permissions.ManagePermissions.Name
    let READ_ROLES = Permissions.ReadRoles.Name
    let MANAGE_ROLES = Permissions.ManageRoles.Name
    let READ_USERS = Permissions.ReadUsers.Name
