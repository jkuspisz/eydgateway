using System.ComponentModel.DataAnnotations;
using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class CreatePLTViewModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [Display(Name = "Format")]
        public string Format { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [Display(Name = "Length of PLT")]
        public string LengthOfPLT { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        [Display(Name = "When and Who Led")]
        public string WhenAndWhoLed { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Brief Outline of Learning")]
        public string BriefOutlineOfLearning { get; set; } = string.Empty;
        
        [Required]
        [MinLength(2)]
        public List<int> SelectedEPAIds { get; set; } = new List<int>();
        
        public List<EPA> AvailableEPAs { get; set; } = new List<EPA>();
    }
    
    public class PLTListViewModel
    {
        public List<PLTSummaryItem> PLTEntries { get; set; } = new List<PLTSummaryItem>();
        public string UserName { get; set; } = string.Empty;
        public bool CanCreatePLT { get; set; } = false;
        public bool IsViewingOwnPLT { get; set; } = false;
    }
    
    public class PLTSummaryItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string LengthOfPLT { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> LinkedEPAs { get; set; } = new List<string>();
        public bool IsLocked { get; set; } = true;
    }
    
    public class PLTDetailViewModel
    {
        public ProtectedLearningTime PLT { get; set; } = new ProtectedLearningTime();
        public List<EPA> LinkedEPAs { get; set; } = new List<EPA>();
        public bool CanView { get; set; } = false;
        public bool IsOwner { get; set; } = false;
        public string AuthorName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
