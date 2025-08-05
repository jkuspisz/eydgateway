using System.ComponentModel.DataAnnotations;
using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class CreateLearningNeedViewModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Learning Need Name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Date Identified")]
        public DateTime DateIdentified { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Display(Name = "Learning Objectives")]
        public string LearningObjectives { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "How to Address")]
        public string HowToAddress { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "When to Meet")]
        public DateTime WhenToMeet { get; set; } = DateTime.UtcNow.AddDays(30);
        
        [Required]
        [Display(Name = "Priority")]
        public LearningNeedPriority Priority { get; set; }
        
        // EYD fills out completion fields during creation (can be empty initially)
        [Display(Name = "The Development/Learning Need was achieved by")]
        public string AchievedBy { get; set; } = string.Empty;
        
        [Display(Name = "Reflection on meeting the development / learning need")]
        public string ReflectionOnMeeting { get; set; } = string.Empty;
        
        [Display(Name = "Date of Achievement")]
        public DateTime? DateOfAchievement { get; set; }
    }
    
    public class LearningNeedListViewModel
    {
        public List<LearningNeedSummaryItem> LearningNeeds { get; set; } = new List<LearningNeedSummaryItem>();
        public string UserName { get; set; } = string.Empty;
        public bool CanCreateLearningNeed { get; set; } = false;
        public bool IsViewingOwnLearningNeeds { get; set; } = false;
    }
    
    public class LearningNeedSummaryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateIdentified { get; set; }
        public LearningNeedPriority Priority { get; set; }
        public LearningNeedStatus Status { get; set; }
        public DateTime WhenToMeet { get; set; }
    }
    
    public class EditLearningNeedViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        [Display(Name = "Learning Need Name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Date Identified")]
        public DateTime DateIdentified { get; set; }
        
        [Required]
        [Display(Name = "Learning Objectives")]
        public string LearningObjectives { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "How to Address")]
        public string HowToAddress { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "When to Meet")]
        public DateTime WhenToMeet { get; set; }
        
        [Required]
        [Display(Name = "Priority")]
        public LearningNeedPriority Priority { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public LearningNeedStatus Status { get; set; }
        
        [Required]
        [Display(Name = "How will this be achieved")]
        public string AchievedBy { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Reflection on Meeting")]
        public string ReflectionOnMeeting { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Date of Achievement")]
        public DateTime DateOfAchievement { get; set; }
    }
    
    public class LearningNeedDetailViewModel
    {
        public LearningNeed LearningNeed { get; set; } = new LearningNeed();
        public bool CanView { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool IsOwner { get; set; } = false;
        public string AuthorName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
