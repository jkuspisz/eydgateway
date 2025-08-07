using System.ComponentModel.DataAnnotations;

namespace EYDGateway.ViewModels
{
    public class SubmitPSQResponseDto
    {
        [Required]
        public string QuestionnaireCode { get; set; } = "";

        // 12 Patient Satisfaction Questions
        [Required]
        public int? PutMeAtEaseScore { get; set; }

        [Required]
        public int? TreatedWithDignityScore { get; set; }

        [Required]
        public int? ListenedToConcernsScore { get; set; }

        [Required]
        public int? ExplainedTreatmentOptionsScore { get; set; }

        [Required]
        public int? InvolvedInDecisionsScore { get; set; }

        [Required]
        public int? InvolvedFamilyScore { get; set; }

        [Required]
        public int? TailoredApproachScore { get; set; }

        [Required]
        public int? ExplainedNextStepsScore { get; set; }

        [Required]
        public int? ProvidedGuidanceScore { get; set; }

        [Required]
        public int? AllocatedTimeScore { get; set; }

        [Required]
        public int? WorkedWithTeamScore { get; set; }

        [Required]
        public int? CanTrustDentistScore { get; set; }

        // Open-ended feedback (optional)
        [MaxLength(2000)]
        public string? DoesWellComment { get; set; }

        [MaxLength(2000)]
        public string? CouldImproveComment { get; set; }
    }
}
