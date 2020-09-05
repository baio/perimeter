using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class DomainPool
    {
        public DomainPool()
        {
            Domains = new HashSet<Domain>();
        }

        public int Id { get; set; }

        [Required] public string Name { get; set; }
        
        [Required] public string Identifier { get; set; }

        public int TenantId { get; set; }

        public Tenant Tenant { get; set; }

        public virtual ICollection<Domain> Domains { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }
    }
}