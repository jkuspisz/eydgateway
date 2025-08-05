using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class SLE
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string SLEType { get; set; } = string.Empty; // CBD, DOPS, MiniCEX, DOPSSim, DtCT, DENTL
        
        [Required]
        [MaxLength(450)]
        public string EYDUserId { get; set; } = string.Empty; // EYD who creates the SLE
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime ScheduledDate { get; set; }
        
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty; // For CBD, DOPS, Simulated DOPS, Mini-CEX
        
        [MaxLength(200)]
        public string Setting { get; set; } = string.Empty; // For DENTL
        
        [MaxLength(200)]
        public string Audience { get; set; } = string.Empty; // For DtCT
        
        [MaxLength(200)]
        public string AudienceSetting { get; set; } = string.Empty; // For DtCT combined field
        
        // Assessor Information
        [MaxLength(450)]
        public string? AssessorUserId { get; set; } // Internal assessor (ES/TPD)
        
        [MaxLength(200)]
        public string? ExternalAssessorName { get; set; } // External assessor name
        
        [MaxLength(255)]
        public string? ExternalAssessorEmail { get; set; } // External assessor email
        
        [MaxLength(100)]
        public string? ExternalAssessorInstitution { get; set; } // External assessor institution
        
        // Assessment Status & Invitation
        public bool IsInternalAssessor { get; set; } = true; // true = internal ES/TPD, false = external
        
        [MaxLength(36)]
        public string? ExternalAccessToken { get; set; } // Unique token for external access
        
        public DateTime? InvitationSentAt { get; set; }
        
        public bool IsAssessmentCompleted { get; set; } = false;
        
        public DateTime? AssessmentCompletedAt { get; set; }
        
        // Assessment Content
        [MaxLength(2000)]
        public string? BehaviourFeedback { get; set; } // Feedback based on the behaviours observed
        
        [MaxLength(1000)]
        public string? AgreedAction { get; set; } // Agreed action
        
        [MaxLength(200)]
        public string? AssessorPosition { get; set; } // Assessor's Position
        
        public int? AssessmentRating { get; set; } // 1-5 scale or similar (if still needed)
        
        [MaxLength(1000)]
        public string? ReflectionNotes { get; set; } // EYD's reflection after assessment
        
        public DateTime? ReflectionCompletedAt { get; set; }
        
        // Status Management
        public string Status { get; set; } = "Draft"; // Draft, Sent, AssessmentCompleted, ReflectionCompleted, Cancelled
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser EYDUser { get; set; } = null!;
        public virtual ApplicationUser? AssessorUser { get; set; }
        public virtual ICollection<EPAMapping> EPAMappings { get; set; } = new List<EPAMapping>();
    }
}
