using System.ComponentModel.DataAnnotations;

namespace EYDGateway.ViewModels
{
    public class CreateSignificantEventViewModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Event Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Account of Experience")]
        public string AccountOfExperience { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Analysis of the Situation")]
        public string AnalysisOfSituation { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Reflection on the Event")]
        public string ReflectionOnEvent { get; set; } = string.Empty;
        
        [Display(Name = "Selected EPAs")]
        public List<int> SelectedEPAIds { get; set; } = new List<int>();
    }
    
    public class EditSignificantEventViewModel : CreateSignificantEventViewModel
    {
        public int Id { get; set; }
        public bool IsLocked { get; set; }
        public bool ESSignedOff { get; set; }
        public bool TPDSignedOff { get; set; }
        public DateTime? ESSignedOffAt { get; set; }
        public DateTime? TPDSignedOffAt { get; set; }
        public string? ESUserName { get; set; }
        public string? TPDUserName { get; set; }
    }
    
    public class SignificantEventDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AccountOfExperience { get; set; } = string.Empty;
        public string AnalysisOfSituation { get; set; } = string.Empty;
        public string ReflectionOnEvent { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool ESSignedOff { get; set; }
        public bool TPDSignedOff { get; set; }
        public DateTime? ESSignedOffAt { get; set; }
        public DateTime? TPDSignedOffAt { get; set; }
        public string? ESUserName { get; set; }
        public string? TPDUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<EPAMappingViewModel> EPAMappings { get; set; } = new List<EPAMappingViewModel>();
    }
    
    public class SignificantEventIndexViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool ESSignedOff { get; set; }
        public bool TPDSignedOff { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int EPACount { get; set; }
        public List<EPAMappingViewModel> EPAs { get; set; } = new List<EPAMappingViewModel>();
    }

    public class EPAMappingViewModel
    {
        public int Id { get; set; }
        public int EPAId { get; set; }
        public string EPACode { get; set; } = string.Empty;
        public string EPATitle { get; set; } = string.Empty;
        public string EPADescription { get; set; } = string.Empty;
    }
}
