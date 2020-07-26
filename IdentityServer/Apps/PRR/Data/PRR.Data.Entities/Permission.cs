using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class Permission
    {
        public Permission()
        {
            RolesPermissions = new HashSet<RolePermission>();
        }

        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public  bool IsTenantManagement { get; set; }

        [Required]
        public  bool IsDomainManagement { get; set; }
        
        [Required]
        public  bool IsCompanyFriendly { get; set; }
        
        [Required]
        public DateTime DateCreated { get; set; }
        
        public virtual ICollection<RolePermission> RolesPermissions { get; set; }
        
        public int? ApiId { get; set; }
        
        public Api Api { get; set; } 
    }
}