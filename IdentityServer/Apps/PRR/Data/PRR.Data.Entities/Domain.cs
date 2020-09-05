using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class Domain
    {
        public Domain()
        {
            Applications = new HashSet<Application>();
            Apis = new HashSet<Api>();
            DomainUsersRoles = new HashSet<DomainUserRole>();
            Roles = new HashSet<Role>();
        }

        public int Id { get; set; }

        [Required] public string EnvName { get; set; }

        // Common domain
        public int? PoolId { get; set; }

        public DomainPool Pool { get; set; }

        // Tenant management
        public int? TenantId { get; set; }

        public Tenant Tenant { get; set; }

        public virtual ICollection<Application> Applications { get; set; }

        public virtual ICollection<Api> Apis { get; set; }

        [Required] public bool IsMain { get; set; }

        [Required] public DateTime DateCreated { get; set; }

        [Required] public string Issuer { get; set; }

        public virtual ICollection<DomainUserRole> DomainUsersRoles { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
    }
}