using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class Tenant
    {
        public Tenant()
        {
            DomainPools = new HashSet<DomainPool>();
        }

        public int Id { get; set; }

        [Required] public string Name { get; set; }

        [Required] public DateTime DateCreated { get; set; }

        [Required] public int UserId { get; set; }

        public User User { get; set; }
        
        public virtual ICollection<DomainPool> DomainPools { get; set; }
    }
}