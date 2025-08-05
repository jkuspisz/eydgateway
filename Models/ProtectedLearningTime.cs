using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class ProtectedLearningTime
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Format { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LengthOfPLT { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string WhenAndWhoLed { get; set; } = string.Empty;
        
        [Required]
        public string BriefOutlineOfLearning { get; set; } = string.Empty;
        
        public bool IsLocked { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<EPAMapping> EPAMappings { get; set; } = new List<EPAMapping>();
    }
}
