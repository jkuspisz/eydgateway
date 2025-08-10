using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class AdHocESReportEPAAssessment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdHocESReportId { get; set; }

        [Required]
        public int EPAId { get; set; }

        [MaxLength(50)]
        public string? ProgressLevel { get; set; } // "Working towards", "Meets", "Exceeds", "Not observed"

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("AdHocESReportId")]
        public virtual AdHocESReport? AdHocESReport { get; set; }

        [ForeignKey("EPAId")]
        public virtual EPA? EPA { get; set; }
    }
}
