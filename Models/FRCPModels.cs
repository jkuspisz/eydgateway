using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public enum FRCPStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

    public class FRCPReview
    {
        public int Id { get; set; }
        
        [Required]
        public string EYDUserId { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        // Workflow status tracking
        public FRCPStatus ESStatus { get; set; } = FRCPStatus.NotStarted;
        public FRCPStatus EYDStatus { get; set; } = FRCPStatus.NotStarted;
        public FRCPStatus PanelStatus { get; set; } = FRCPStatus.NotStarted;
        
        // Lock status for each section
        public bool ESLocked { get; set; } = false;
        public bool EYDLocked { get; set; } = false;
        public bool PanelLocked { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<FRCPESAssessment> ESAssessments { get; set; } = new List<FRCPESAssessment>();
        public virtual FRCPESSection? ESSection { get; set; }
        public virtual FRCPEYDReflection? EYDReflection { get; set; }
        public virtual FRCPPanelReview? PanelReview { get; set; }
        
        // Foreign key navigation
        [ForeignKey("EYDUserId")]
        public virtual ApplicationUser? EYDUser { get; set; }
    }

    public class FRCPESAssessment
    {
        public int Id { get; set; }
        
        [Required]
        public int FRCPReviewId { get; set; }
        
        [Required]
        public int EPAId { get; set; }
        
        public int EntrustmentLevel { get; set; }
        
        [Required]
        public string Justification { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("FRCPReviewId")]
        public virtual FRCPReview? FRCPReview { get; set; }
        
        [ForeignKey("EPAId")]
        public virtual EPA? EPA { get; set; }
    }

    public class FRCPESSection
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int FRCPReviewId { get; set; }
        
        // ES Assessment Fields
        public string? OverallAssessment { get; set; }
        public string? NotablePractice { get; set; }
        public string? PerformanceConcerns { get; set; }
        public string? DevelopmentPriorities { get; set; }
        public bool ConfirmAccuracy { get; set; } = false;
        
        // Final Review specific fields
        public bool? ReadyForIndependentPractice { get; set; }
        public string? AdditionalComments { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
        
        [ForeignKey("FRCPReviewId")]
        public virtual FRCPReview FRCPReview { get; set; } = null!;
    }

    public class FRCPEYDReflection
    {
        public int Id { get; set; }
        
        [Required]
        public int FRCPReviewId { get; set; }
        
        // Text fields
        public string ProgressSummary { get; set; } = string.Empty;
        public string ChallengesFaced { get; set; } = string.Empty;
        public string LearningGoals { get; set; } = string.Empty;
        public string SupportNeeded { get; set; } = string.Empty;
        
        // Yes/No fields
        public bool ReadyForNextStage { get; set; }
        public string ReadyForNextStageExplanation { get; set; } = string.Empty;
        
        public bool AdditionalTrainingNeeded { get; set; }
        public string AdditionalTrainingDetails { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("FRCPReviewId")]
        public virtual FRCPReview? FRCPReview { get; set; }
    }

    public class FRCPPanelReview
    {
        public int Id { get; set; }
        
        [Required]
        public int FRCPReviewId { get; set; }
        
        // Panel decision fields
        public string OverallAssessment { get; set; } = string.Empty;
        public string RecommendedActions { get; set; } = string.Empty;
        public string ProgressPlan { get; set; } = string.Empty;
        public string NextReviewDate { get; set; } = string.Empty;
        
        // Panel consensus
        public bool PanelConsensusReached { get; set; }
        public string ConsensusNotes { get; set; } = string.Empty;
        
        // Final decision
        public string FinalDecision { get; set; } = string.Empty;
        public string DecisionRationale { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("FRCPReviewId")]
        public virtual FRCPReview? FRCPReview { get; set; }
    }
}
