using System;
using System.ComponentModel.DataAnnotations;

namespace PRR.Data.Entities
{
    public enum FlowType
    {
        PKCE,
        AuthorizationCode,
        Implicit
    }

    public class Application
    {
        public int Id { get; set; }

        [Required] public string Name { get; set; }

        public int DomainId { get; set; }

        public Domain Domain { get; set; }

        [Required] public string ClientId { get; set; }

        [Required] public string ClientSecret { get; set; }

        [Required] public DateTime DateCreated { get; set; }

        [Required] public int IdTokenExpiresIn { get; set; }

        [Required] public int RefreshTokenExpiresIn { get; set; }

        [Required] public string AllowedCallbackUrls { get; set; }
        
        [Required] public string AllowedLogoutCallbackUrls { get; set; }

        [Required] public bool SSOEnabled { get; set; }

        [Required] public FlowType Flow { get; set; }

        [Required] public bool IsDomainManagement { get; set; }
    }
}