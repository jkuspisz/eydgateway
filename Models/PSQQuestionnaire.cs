using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class PSQQuestionnaire
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PerformerId { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = "";

        [Required]
        [MaxLength(8)]
        public string UniqueCode { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PerformerId")]
        public virtual ApplicationUser? Performer { get; set; }

        public virtual ICollection<PSQResponse> Responses { get; set; } = new List<PSQResponse>();
    }
}
