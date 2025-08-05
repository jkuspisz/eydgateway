using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class ClinicalExperienceLog
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty; // Critical: User ownership
        
        [Required]
        public DateTime ExperienceDate { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string ClinicalSetting { get; set; } = string.Empty; // Hospital, Clinic, Practice, etc.
        
        [Required]
        [MaxLength(200)]
        public string Department { get; set; } = string.Empty; // Oral Surgery, Orthodontics, etc.
        
        [Required]
        [MaxLength(200)]
        public string SupervisorName { get; set; } = string.Empty;
        
        [Required]
        public string ClinicalActivities { get; set; } = string.Empty; // What procedures/activities
        
        [MaxLength(500)]
        public string? PatientsSeenDetails { get; set; } // Number and types of patients
        
        [MaxLength(500)]
        public string? ProceduresPerformed { get; set; }
        
        [MaxLength(500)]
        public string? LearningPoints { get; set; }
        
        [MaxLength(500)]
        public string? ChallengesFaced { get; set; }
        
        [MaxLength(500)]
        public string? SupervisorFeedback { get; set; }
        
        public decimal DurationHours { get; set; } = 8.0m; // Default full day
        
        public bool IsVerified { get; set; } = false; // Supervisor verification
        
        public DateTime? VerifiedAt { get; set; }
        
        [MaxLength(450)]
        public string? VerifiedByUserId { get; set; } // Supervisor who verified
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationUser? VerifiedByUser { get; set; }
        public virtual ICollection<EPAMapping> EPAMappings { get; set; } = new List<EPAMapping>();
    }
}
