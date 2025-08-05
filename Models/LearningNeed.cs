using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class LearningNeed
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        public DateTime DateIdentified { get; set; }
        
        [Required]
        public string LearningObjectives { get; set; } = string.Empty;
        
        [Required]
        public string HowToAddress { get; set; } = string.Empty;
        
        public DateTime WhenToMeet { get; set; }
        
        public LearningNeedPriority Priority { get; set; }
        
        public string AchievedBy { get; set; } = string.Empty;
        
        public string ReflectionOnMeeting { get; set; } = string.Empty;
        
        public DateTime DateOfAchievement { get; set; }
        
        public LearningNeedStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? SubmittedAt { get; set; }
        
        [MaxLength(450)]
        public string? CompletedByUserId { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationUser? CompletedByUser { get; set; }
    }
    
    public enum LearningNeedPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
    
    public enum LearningNeedStatus
    {
        Draft = 0,          // EYD user is still working on it
        Submitted = 1,      // EYD user has submitted it for review
        Completed = 2,      // ES/TPD has marked it as complete
        Deferred = 3        // Deferred for later
    }
}
