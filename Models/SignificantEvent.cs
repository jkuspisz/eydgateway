using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class SignificantEvent
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        [Display(Name = "Event Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Account of Experience")]
        public string AccountOfExperience { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Analysis of the Situation")]
        public string AnalysisOfSituation { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Reflection on the Event")]
        public string ReflectionOnEvent { get; set; } = string.Empty;
        
        public bool IsLocked { get; set; } = false;
        
        [Display(Name = "ES Signed Off")]
        public bool ESSignedOff { get; set; } = false;
        
        [Display(Name = "ES Signed Off At")]
        public DateTime? ESSignedOffAt { get; set; }
        
        [MaxLength(450)]
        [Display(Name = "ES User ID")]
        public string? ESUserId { get; set; }
        
        [Display(Name = "TPD Signed Off")]
        public bool TPDSignedOff { get; set; } = false;
        
        [Display(Name = "TPD Signed Off At")]
        public DateTime? TPDSignedOffAt { get; set; }
        
        [MaxLength(450)]
        [Display(Name = "TPD User ID")]
        public string? TPDUserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationUser? ESUser { get; set; }
        public virtual ApplicationUser? TPDUser { get; set; }
        // Note: EPA mappings are handled through the generic EPAMapping table with EntityType = "SignificantEvent"
    }
}
