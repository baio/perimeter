using System.Collections.Generic;

namespace PRR.Data.DataContext.Seed
{
    using PRR.Data.Entities;

    public static class Roles
    {
        public static readonly Role TenantOwner = new Role
            {Id = -100, Name = "TenantOwner", Description = "Tenant owner", IsTenantManagement = true};

        public static readonly Role TenantSuperAdmin = new Role
            {Id = -200, Name = "TenantSuperAdmin", Description = "Tenant Super admin", IsTenantManagement = true};

        public static readonly Role TenantAdmin = new Role
            {Id = -300, Name = "TenantAdmin", Description = "Tenant Admin", IsTenantManagement = true};

        public static readonly Role DomainOwner = new Role
            {Id = -400, Name = "DomainOwner", Description = "Domain owner", IsDomainManagement = true};

        public static readonly Role DomainSuperAdmin = new Role
            {Id = -500, Name = "DomainSuperAdmin", Description = "Domain Super admin", IsDomainManagement = true};

        public static readonly Role DomainAdmin = new Role
            {Id = -600, Name = "DomainAdmin", Description = "Domain Admin", IsDomainManagement = true};

        public static IEnumerable<Role> GetAll()
        {
            yield return TenantOwner;
            yield return TenantSuperAdmin;
            yield return TenantAdmin;

            yield return DomainOwner;
            yield return DomainSuperAdmin;
            yield return DomainAdmin;
        }
    }
}