using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class PortfolioReflection  // Renamed to avoid conflict with System.Reflection
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty; // Critical: User ownership
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty; // Focus/Title of Reflection
        
        [Required]
        [MaxLength(500)]
        public string WhenDidItHappen { get; set; } = string.Empty; // When did it happen? (txt field)
        
        [Required]
        public string ReasonsForWriting { get; set; } = string.Empty; // Reasons for writing the reflection
        
        [Required]
        public string NextSteps { get; set; } = string.Empty; // Next steps
        
        public bool IsLocked { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<EPAMapping> EPAMappings { get; set; } = new List<EPAMapping>();
    }
}
