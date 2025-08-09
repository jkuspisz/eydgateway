using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class MSFResponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MSFQuestionnaireId { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Communication Topic (4 questions)
        public int? TreatWithCompassionScore { get; set; }
        public int? EnableInformedDecisionsScore { get; set; }
        public int? RecogniseCommunicationNeedsScore { get; set; }
        public int? ProduceClearCommunicationsScore { get; set; }

        // Professionalism Topic (7 questions)
        public int? DemonstrateIntegrityScore { get; set; }
        public int? WorkWithinScopeScore { get; set; }
        public int? EngageWithDevelopmentScore { get; set; }
        public int? KeepPracticeUpToDateScore { get; set; }
        public int? FacilitateLearningScore { get; set; }
        public int? InteractWithColleaguesScore { get; set; }
        public int? PromoteEqualityScore { get; set; }

        // Management and Leadership Topic (6 questions)
        public int? RecogniseImpactOfBehavioursScore { get; set; }
        public int? ManageTimeAndResourcesScore { get; set; }
        public int? WorkAsTeamMemberScore { get; set; }
        public int? WorkToStandardsScore { get; set; }
        public int? ParticipateInImprovementScore { get; set; }
        public int? MinimiseWasteScore { get; set; }

        // Text feedback
        [MaxLength(2000)]
        public string? DoesWellComment { get; set; }
        
        [MaxLength(2000)]
        public string? CouldImproveComment { get; set; }

        // Navigation property
        [ForeignKey("MSFQuestionnaireId")]
        public virtual MSFQuestionnaire? Questionnaire { get; set; }
    }
}
