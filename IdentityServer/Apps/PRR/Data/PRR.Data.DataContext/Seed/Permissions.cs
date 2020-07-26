using System.Collections.Generic;

namespace PRR.Data.DataContext.Seed
{
    using PRR.Data.Entities;

    public static class Permissions
    {
        public static readonly Permission ArchiveTenant = new Permission
        {
            Id = -50, Name = "archive:tenant", Description = "Allow archive tenant", IsTenantManagement = true
        };

        public static readonly Permission ManageTenantAdmins = new Permission
        {
            Id = -100, Name = "manage:tenant-admins", Description = "Allow manage tenant admins", IsTenantManagement = true
        };

        public static readonly Permission ManageTenantDomains = new Permission
        {
            Id = -200, Name = "manage:tenant-domains", Description = "Allow manage tenant domains", IsTenantManagement = true
        };

        public static readonly Permission ArchiveDomain = new Permission
            {Id = -250, Name = "archive:domain", Description = "Archive domain", IsDomainManagement = true};

        public static readonly Permission ManageDomain = new Permission
            {Id = -300, Name = "manage:domain", Description = "Manage domain properties", IsDomainManagement = true};
        
        public static readonly Permission ManageDomainSuperAdmins = new Permission
        {
            Id = -400, Name = "manage:domain-super-admins", Description = "Allow manage domain super admins", IsDomainManagement = true
        };

        public static readonly Permission ManageDomainAdmins = new Permission
        {
            Id = -500, Name = "manage:domain-admins", Description = "Allow manage domain admins", IsDomainManagement = true
        };

        public static readonly Permission ManageUsers = new Permission
        {
            Id = -600, Name = "manage:users", Description = "Manage users except admins, super-admins and owners",
            IsDomainManagement = true
        };
        
        public static readonly Permission ReadUsers = new Permission
            {Id = -700, Name = "read:users", Description = "Read users", IsDomainManagement = true};

        public static readonly Permission ReadRoles = new Permission
            {Id = -800, Name = "read:roles", Description = "Read roles", IsDomainManagement = true};

        public static readonly Permission ManageRoles = new Permission
            {Id = -900, Name = "manage:roles", Description = "Manage roles", IsDomainManagement = true};

        public static readonly Permission ReadPermissions = new Permission
            {Id = -1000, Name = "read:permissions", Description = "Read permissions", IsDomainManagement = true};

        public static readonly Permission ManagePermissions = new Permission
            {Id = -1100, Name = "manage:permissions", Description = "Manage permissions", IsDomainManagement = true};
        
        public static IEnumerable<Permission> GetAll()
        {
            yield return ArchiveTenant;
            yield return ManageTenantAdmins;
            yield return ManageTenantDomains;
            yield return ArchiveDomain;
            yield return ManageDomain;
            yield return ManageDomainSuperAdmins;
            yield return ManageDomainAdmins;
            yield return ManageUsers;
            yield return ReadUsers;
            yield return ReadRoles;
            yield return ManageRoles;
            yield return ReadPermissions;
            yield return ManagePermissions;
        }
    }
    
}