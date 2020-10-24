using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class User
    {
        public User()
        {
            DomainUsersRoles = new HashSet<DomainUserRole>();
            SocialIdentities = new HashSet<SocialIdentity>();
        }

        public int Id { get; set; }

        [Required] public string FirstName { get; set; }

        [Required] public string LastName { get; set; }

        [EmailAddress] public string Email { get; set; }

        public string Password { get; set; }

        [Required] public DateTime DateCreated { get; set; }

        public virtual ICollection<DomainUserRole> DomainUsersRoles { get; set; }

        public virtual ICollection<SocialIdentity> SocialIdentities { get; set; }
    }
}