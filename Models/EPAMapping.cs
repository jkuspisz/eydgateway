using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class EPAMapping
    {
        public int Id { get; set; }
        
        [Required]
        public int EPAId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty; // "Reflection", "SLE", "ProtectedLearningTime", "SignificantEvent", "QIUpload"
        
        [Required]
        public int EntityId { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual EPA EPA { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
