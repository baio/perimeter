using System;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class Application
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public int DomainId { get; set; }
        
        public Domain Domain { get; set; }
        
        [Required]
        public string ClientId { get; set; }
        
        [Required]
        public string ClientSecret { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }
        
        [Required]
        public int IdTokenExpiresIn { get; set; }
        
        [Required]
        public int RefreshTokenExpiresIn { get; set; }

    }
}