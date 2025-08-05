using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class LearningLog
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty; // Critical: User ownership
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LogType { get; set; } = string.Empty; // Protected, Clinical, Research, Training, etc.
        
        public DateTime LogDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string Description { get; set; } = string.Empty; // What was learned/done
        
        [MaxLength(500)]
        public string? LearningObjectives { get; set; }
        
        [MaxLength(500)]
        public string? Outcomes { get; set; }
        
        [MaxLength(500)]
        public string? ReflectionNotes { get; set; }
        
        public decimal? DurationHours { get; set; } // For protected learning time tracking
        
        [MaxLength(200)]
        public string? Location { get; set; }
        
        [MaxLength(200)]
        public string? Supervisor { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Optional relationships
        public int? RelatedSLEId { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual SLE? RelatedSLE { get; set; }
        public virtual ICollection<EPAMapping> EPAMappings { get; set; } = new List<EPAMapping>();
    }
}
