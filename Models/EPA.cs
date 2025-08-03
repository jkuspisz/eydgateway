using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class EPA
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property for mappings
        public virtual ICollection<EPAMapping> EPAMappings { get; set; } = new List<EPAMapping>();
    }
}
