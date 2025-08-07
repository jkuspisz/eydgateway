using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class PSQResponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PSQQuestionnaireId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // 12 Patient Satisfaction Questions (PSQ scoring: -1, -2, 3, 4, or 999 for "Not observed")
        public int? PutMeAtEaseScore { get; set; }                    // The Dentist put me at ease
        public int? TreatedWithDignityScore { get; set; }            // Treated me with dignity and respect
        public int? ListenedToConcernsScore { get; set; }            // Listened and responded to my concerns
        public int? ExplainedTreatmentOptionsScore { get; set; }     // Clearly explained treatment options including costs
        public int? InvolvedInDecisionsScore { get; set; }           // Involved me in decisions about my care
        public int? InvolvedFamilyScore { get; set; }                // Involved family/carers appropriately
        public int? TailoredApproachScore { get; set; }              // Tailored approach to meet my needs
        public int? ExplainedNextStepsScore { get; set; }            // Explained what will happen next with treatment
        public int? ProvidedGuidanceScore { get; set; }              // Provided guidance on dental care
        public int? AllocatedTimeScore { get; set; }                 // Allocated right amount of time for treatment
        public int? WorkedWithTeamScore { get; set; }                // Worked well with other team members
        public int? CanTrustDentistScore { get; set; }               // Can trust this dentist with dental care

        // 2 Open-ended text feedback questions
        [MaxLength(2000)]
        public string? DoesWellComment { get; set; }                 // What dentist does particularly well

        [MaxLength(2000)]
        public string? CouldImproveComment { get; set; }             // What dentist could improve upon

        // Navigation properties
        [ForeignKey("PSQQuestionnaireId")]
        public virtual PSQQuestionnaire? Questionnaire { get; set; }
    }
}
