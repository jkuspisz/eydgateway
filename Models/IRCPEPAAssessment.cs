using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class IRCPEPAAssessment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InterimReviewId { get; set; }

        [Required]
        public int EPAId { get; set; }

        // ES Assessment Fields
        [Range(1, 4)]
        public int? LevelOfEntrustment { get; set; }

        [MaxLength(2000)]
        public string? Justification { get; set; }

        public DateTime? AssessedAt { get; set; }

        // Navigation properties
        [ForeignKey("InterimReviewId")]
        public virtual InterimReview? InterimReview { get; set; }

        [ForeignKey("EPAId")]
        public virtual EPA? EPA { get; set; }
    }
}
