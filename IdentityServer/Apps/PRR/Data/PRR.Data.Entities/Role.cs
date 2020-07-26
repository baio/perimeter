using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class Role
    {
        public Role()
        {
            RolesPermissions = new HashSet<RolePermission>();

            DomainUsersRoles = new HashSet<DomainUserRole>();
        }

        public int Id { get; set; }

        [Required] public string Name { get; set; }

        [Required] public string Description { get; set; }

        [Required] public bool IsTenantManagement { get; set; }
        
        [Required] public bool IsDomainManagement { get; set; }

        [Required] public bool IsDefault { get; set; }

        [Required] public DateTime DateCreated { get; set; }

        public virtual ICollection<DomainUserRole> DomainUsersRoles { get; set; }
        public virtual ICollection<RolePermission> RolesPermissions { get; set; }

        public int? DomainId { get; set; }

        public Domain Domain { get; set; }
    }
}