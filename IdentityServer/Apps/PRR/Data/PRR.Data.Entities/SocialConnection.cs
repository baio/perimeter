using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public class SocialConnection
    {
        [Required] public int DomainId { get; set; }

        public Domain Domain { get; set; }

        [Required] public string SocialName { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string[] Attributes { get; set; }

        public string[] Permissions { get; set; }
    }
}