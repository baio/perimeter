using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class Api
    {
        public Api()
        {
            Permissions = new HashSet<Permission>();
        }

        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public int DomainId { get; set; }
        
        public Domain Domain { get; set; }
        
        [Required]
        // This is Audience
        public string Identifier { get; set; }
        
        [Required]
        public bool IsDomainManagement { get; set; }
        
        [Required]
        public DateTime DateCreated { get; set; }
        
        public virtual ICollection<Permission> Permissions { get; set; }
        
        [Required]
        public int AccessTokenExpiresIn { get; set; }
    }
}