using System.ComponentModel.DataAnnotations;

namespace EYDGateway.ViewModels
{
    public class SubmitMSFResponseDto
    {
        [Required]
        public string QuestionnaireCode { get; set; } = "";

        // Communication Topic (4 questions) - All required
        [Required] public int? TreatWithCompassionScore { get; set; }
        [Required] public int? EnableInformedDecisionsScore { get; set; }
        [Required] public int? RecogniseCommunicationNeedsScore { get; set; }
        [Required] public int? ProduceClearCommunicationsScore { get; set; }

        // Professionalism Topic (7 questions) - All required
        [Required] public int? DemonstrateIntegrityScore { get; set; }
        [Required] public int? WorkWithinScopeScore { get; set; }
        [Required] public int? EngageWithDevelopmentScore { get; set; }
        [Required] public int? KeepPracticeUpToDateScore { get; set; }
        [Required] public int? FacilitateLearningScore { get; set; }
        [Required] public int? InteractWithColleaguesScore { get; set; }
        [Required] public int? PromoteEqualityScore { get; set; }

        // Management and Leadership Topic (6 questions) - All required
        [Required] public int? RecogniseImpactOfBehavioursScore { get; set; }
        [Required] public int? ManageTimeAndResourcesScore { get; set; }
        [Required] public int? WorkAsTeamMemberScore { get; set; }
        [Required] public int? WorkToStandardsScore { get; set; }
        [Required] public int? ParticipateInImprovementScore { get; set; }
        [Required] public int? MinimiseWasteScore { get; set; }

        // Optional text feedback
        [MaxLength(2000)]
        public string? DoesWellComment { get; set; }
        
        [MaxLength(2000)]
        public string? CouldImproveComment { get; set; }
    }
}
