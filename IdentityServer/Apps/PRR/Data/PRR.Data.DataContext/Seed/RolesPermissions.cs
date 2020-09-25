using System.Collections.Generic;
using System.Linq;

namespace PRR.Data.DataContext.Seed
{
    using PRR.Data.Entities;

    public class RolesPermissions
    {
        public static readonly RolePermission[] TenantOwner = new[]
        {
            new RolePermission {RoleId = Roles.TenantOwner.Id, PermissionId = Permissions.ArchiveTenant.Id},
            new RolePermission {RoleId = Roles.TenantOwner.Id, PermissionId = Permissions.ManageTenantDomains.Id},
            new RolePermission {RoleId = Roles.TenantOwner.Id, PermissionId = Permissions.ManageTenantAdmins.Id}
        };

        public static readonly RolePermission[] TenantSuperAdmin = new[]
        {
            new RolePermission {RoleId = Roles.TenantSuperAdmin.Id, PermissionId = Permissions.ManageTenantDomains.Id},
            new RolePermission {RoleId = Roles.TenantSuperAdmin.Id, PermissionId = Permissions.ManageTenantAdmins.Id}
        };

        public static readonly RolePermission[] TenantAdmin = new[]
        {
            new RolePermission {RoleId = Roles.TenantAdmin.Id, PermissionId = Permissions.ManageTenantDomains.Id}
        };

        public static readonly RolePermission[] DomainOwner = new[]
        {
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ArchiveDomain.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ManageDomain.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ManageDomainSuperAdmins.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ManageDomainAdmins.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ManageUsers.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ReadUsers.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ReadRoles.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ManageRoles.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ReadPermissions.Id},
            new RolePermission {RoleId = Roles.DomainOwner.Id, PermissionId = Permissions.ManagePermissions.Id}
        };

        public static readonly RolePermission[] DomainSuperAdmin = new[]
        {
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ManageDomain.Id},
            new RolePermission
                {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ManageDomainSuperAdmins.Id},
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ManageDomainAdmins.Id},
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ManageUsers.Id},
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ReadUsers.Id},
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ReadRoles.Id},
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ManageRoles.Id},
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ReadPermissions.Id},
            new RolePermission {RoleId = Roles.DomainSuperAdmin.Id, PermissionId = Permissions.ManagePermissions.Id}
        };

        public static readonly RolePermission[] DomainAdmin = new[]
        {
            new RolePermission {RoleId = Roles.DomainAdmin.Id, PermissionId = Permissions.ManageDomain.Id},
            new RolePermission {RoleId = Roles.DomainAdmin.Id, PermissionId = Permissions.ManageUsers.Id},
            new RolePermission {RoleId = Roles.DomainAdmin.Id, PermissionId = Permissions.ReadUsers.Id},
            new RolePermission {RoleId = Roles.DomainAdmin.Id, PermissionId = Permissions.ReadRoles.Id},
            new RolePermission {RoleId = Roles.DomainAdmin.Id, PermissionId = Permissions.ManageRoles.Id},
            new RolePermission {RoleId = Roles.DomainAdmin.Id, PermissionId = Permissions.ReadPermissions.Id},
            new RolePermission {RoleId = Roles.DomainAdmin.Id, PermissionId = Permissions.ManagePermissions.Id}
        };

        public static IEnumerable<RolePermission> GetAll()
        {
            return TenantOwner.Concat(TenantSuperAdmin).Concat(TenantAdmin).Concat(DomainOwner).Concat(DomainSuperAdmin)
                .Concat(DomainAdmin);
        }
    }
}