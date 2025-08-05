using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class DocumentUpload
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty; // Critical: User ownership
        
        [Required]
        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty; // Actual file location
        
        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty; // MIME type
        
        [Required]
        public long FileSizeBytes { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string UploadCategory { get; set; } = string.Empty; // EYD, ES, TPD, General, etc.
        
        [MaxLength(50)]
        public string? DocumentType { get; set; } // Certificate, Report, Portfolio, etc.
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true; // Soft delete support
        
        // Optional relationship to other entities
        public int? RelatedSLEId { get; set; }
        public int? RelatedReflectionId { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual SLE? RelatedSLE { get; set; }
        public virtual PortfolioReflection? RelatedReflection { get; set; }
        public virtual ICollection<EPAMapping> EPAMappings { get; set; } = new List<EPAMapping>();
    }
}
