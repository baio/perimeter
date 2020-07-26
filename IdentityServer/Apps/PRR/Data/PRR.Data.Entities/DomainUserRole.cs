using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace PRR.Data.Entities
{
    public class DomainUserRole : ValueObject
    {
        public int DomainId { get; set; }

        public Domain Domain { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; }

        public string UserEmail { get; set; }
        
        public User User { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DomainId;
            yield return RoleId;
            yield return UserEmail;
        }
    }
}