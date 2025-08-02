using Microsoft.AspNetCore.Identity;

namespace EYDGateway.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Custom fields for your users
        public string DisplayName { get; set; }
        public string Role { get; set; }
        
        // Area assignment (for Admins mainly)
        public int? AreaId { get; set; }
        public Area? Area { get; set; }
        
        // NEW: Scheme assignment (for EYDs and TPDs)
        public int? SchemeId { get; set; }
        public Scheme? Scheme { get; set; }
    }
}
