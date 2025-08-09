using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class IRCPReview
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string EYDUserId { get; set; } = string.Empty;
        
        [Required]
        public string EYDUserName { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
        
        // Workflow Status
        public IRCPStatus ESStatus { get; set; } = IRCPStatus.NotStarted;
        public IRCPStatus EYDStatus { get; set; } = IRCPStatus.NotStarted;
        public IRCPStatus PanelStatus { get; set; } = IRCPStatus.NotStarted;
        
        // Lock Status (for admin override)
        public bool ESLocked { get; set; } = false;
        public bool EYDLocked { get; set; } = false;
        public bool PanelLocked { get; set; } = false;
        
        // Submission Dates
        public DateTime? ESSubmittedDate { get; set; }
        public DateTime? EYDSubmittedDate { get; set; }
        public DateTime? PanelSubmittedDate { get; set; }
        
        // Who submitted each section
        public string? ESSubmittedBy { get; set; }
        public string? EYDSubmittedBy { get; set; }
        public string? PanelSubmittedBy { get; set; }
        
        // Navigation properties
        public virtual ICollection<IRCPESAssessment> ESAssessments { get; set; } = new List<IRCPESAssessment>();
        public virtual IRCPEYDReflection? EYDReflection { get; set; }
        public virtual IRCPPanelReview? PanelReview { get; set; }
    }
    
    public class IRCPESAssessment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int IRCPReviewId { get; set; }
        
        [Required]
        public string EPACode { get; set; } = string.Empty;
        
        public int? EntrustmentLevel { get; set; } // 1-5
        public string? Justification { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
        
        [ForeignKey("IRCPReviewId")]
        public virtual IRCPReview IRCPReview { get; set; } = null!;
    }
    
    public class IRCPESSection
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int IRCPReviewId { get; set; }
        
        // ES Assessment Fields
        public string? OverallAssessment { get; set; }
        public string? NotablePractice { get; set; }
        public string? PerformanceConcerns { get; set; }
        public string? DevelopmentPriorities { get; set; }
        public bool ConfirmAccuracy { get; set; } = false;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
        
        [ForeignKey("IRCPReviewId")]
        public virtual IRCPReview IRCPReview { get; set; } = null!;
    }
    
    public class IRCPEYDReflection
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int IRCPReviewId { get; set; }
        
        public string? Reflection { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
        
        [ForeignKey("IRCPReviewId")]
        public virtual IRCPReview IRCPReview { get; set; } = null!;
    }
    
    public class IRCPPanelReview
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int IRCPReviewId { get; set; }
        
        // Panel Details
        public DateTime? ReviewDate { get; set; }
        public string? PanelMembers { get; set; }
        public string? ExtraDocumentation { get; set; }
        
        // Panel Assessment
        public string? RecommendedOutcome { get; set; }
        public string? DetailedReasons { get; set; }
        public string? MitigatingCircumstances { get; set; }
        public string? CompetenciesToDevelop { get; set; }
        public string? RecommendedActions { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
        
        [ForeignKey("IRCPReviewId")]
        public virtual IRCPReview IRCPReview { get; set; } = null!;
    }
    
    public enum IRCPStatus
    {
        NotStarted = 0,    // Red
        InProgress = 1,    // Amber (saved but not submitted)
        Completed = 2      // Green (submitted)
    }
    
    public class IRCPStatusViewModel
    {
        public string EYDUserId { get; set; } = string.Empty;
        public string EYDUserName { get; set; } = string.Empty;
        public IRCPStatus ESStatus { get; set; }
        public IRCPStatus EYDStatus { get; set; }
        public IRCPStatus PanelStatus { get; set; }
        public DateTime? LastModified { get; set; }
        public int? IRCPReviewId { get; set; }
        
        public string ESStatusClass => ESStatus switch
        {
            IRCPStatus.NotStarted => "badge bg-danger",
            IRCPStatus.InProgress => "badge bg-warning",
            IRCPStatus.Completed => "badge bg-success",
            _ => "badge bg-secondary"
        };
        
        public string EYDStatusClass => EYDStatus switch
        {
            IRCPStatus.NotStarted => "badge bg-danger",
            IRCPStatus.InProgress => "badge bg-warning",
            IRCPStatus.Completed => "badge bg-success",
            _ => "badge bg-secondary"
        };
        
        public string PanelStatusClass => PanelStatus switch
        {
            IRCPStatus.NotStarted => "badge bg-danger",
            IRCPStatus.InProgress => "badge bg-warning",
            IRCPStatus.Completed => "badge bg-success",
            _ => "badge bg-secondary"
        };
        
        public string ESStatusText => ESStatus switch
        {
            IRCPStatus.NotStarted => "Not Started",
            IRCPStatus.InProgress => "In Progress",
            IRCPStatus.Completed => "Completed",
            _ => "Unknown"
        };
        
        public string EYDStatusText => EYDStatus switch
        {
            IRCPStatus.NotStarted => "Not Started",
            IRCPStatus.InProgress => "In Progress",
            IRCPStatus.Completed => "Completed",
            _ => "Unknown"
        };
        
        public string PanelStatusText => PanelStatus switch
        {
            IRCPStatus.NotStarted => "Not Started",
            IRCPStatus.InProgress => "In Progress",
            IRCPStatus.Completed => "Completed",
            _ => "Unknown"
        };
    }
}
