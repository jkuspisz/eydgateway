using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class InterimReview
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string EYDUserId { get; set; } = string.Empty;

        [ForeignKey("EYDUserId")]
        public ApplicationUser EYDUser { get; set; } = null!;
        
        public string? ESUserId { get; set; }

        [ForeignKey("ESUserId")]
        public ApplicationUser? ESUser { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ESCompletedAt { get; set; }
        public DateTime? EYDCompletedAt { get; set; }
        public DateTime? PanelCompletedAt { get; set; }
        
        public bool IsESCompleted { get; set; } = false;
        public bool IsEYDCompleted { get; set; } = false;
        public bool IsPanelCompleted { get; set; } = false;
        
        // Educational Supervisor Section Fields (after EPA Matrix)
        [MaxLength(2000)]
        public string? ESOverallAssessment { get; set; }

        [MaxLength(2000)]
        public string? ESStrengths { get; set; }

        [MaxLength(2000)]
        public string? ESAreasForDevelopment { get; set; }

        [MaxLength(2000)]
        public string? ESRecommendations { get; set; }

        [MaxLength(2000)]
        public string? ESAdditionalComments { get; set; }

        [MaxLength(2000)]
        public string? ESProgressSinceLastReview { get; set; }

        [MaxLength(2000)]
        public string? ESClinicalPerformance { get; set; }

        [MaxLength(2000)]
        public string? ESProfessionalBehavior { get; set; }
        
        // EYD Reflection Section
        [MaxLength(2000)]
        public string? EYDReflectionComments { get; set; }

        [MaxLength(2000)]
        public string? EYDLearningGoals { get; set; }

        [MaxLength(2000)]
        public string? EYDActionPlan { get; set; }
        
        // Panel Section (TPD/Dean access)
        public string? PanelComments { get; set; }
        public string? PanelRecommendations { get; set; }
        public string? PanelOutcome { get; set; }
        
        // Navigation properties for EPA assessments
        public ICollection<IRCPEPAAssessment> EPAAssessments { get; set; } = new List<IRCPEPAAssessment>();
    }
}
