using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace PRR.Data.Entities
{
    public class RolePermission : ValueObject
    {
        public int RoleId { get; set; }

        public Role Role { get; set; }

        public int PermissionId { get; set; }

        public Permission Permission { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return RoleId;
            yield return PermissionId;
        }
    }
}