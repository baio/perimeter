using System;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class SocialIdentity
    {
        public SocialIdentity()
        {
        }

        [Required] public string Name { get; set; }

        [Required] public string Email { get; set; }

        [Required] public string SocialName { get; set; }

        [Required] public string SocialId { get; set; }

        [Required] public DateTime DateCreated { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}