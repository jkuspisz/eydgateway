using System.ComponentModel.DataAnnotations;
using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class ESInductionIndexViewModel
    {
        public ApplicationUser TargetUser { get; set; } = null!;
        public ESInduction? ExistingInduction { get; set; }
    }

    public class ESInductionViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "EYD User")]
        public string EYDUserId { get; set; } = string.Empty;
        public string EYDUserName { get; set; } = string.Empty;

        [Display(Name = "Have you read the Educational Transition Document and agreed a PDP for this placement?")]
        public bool HasReadTransitionDocumentAndAgreedPDP { get; set; }

        [Display(Name = "Comments and notes as discussed and agreed during the meeting")]
        [Required(ErrorMessage = "Please provide comments or notes from the meeting.")]
        [StringLength(2000, ErrorMessage = "Comments cannot exceed 2000 characters.")]
        public string MeetingNotesAndComments { get; set; } = string.Empty;

        [Display(Name = "Brief description of the placement and details")]
        [Required(ErrorMessage = "Please provide a description of the placement.")]
        [StringLength(1000, ErrorMessage = "Placement description cannot exceed 1000 characters.")]
        public string PlacementDescription { get; set; } = string.Empty;

        [Display(Name = "Meeting Date")]
        [DataType(DataType.Date)]
        public DateTime? MeetingDate { get; set; }

        [Display(Name = "Mark as Completed")]
        public bool IsCompleted { get; set; } = false;
    }

    public class CreateESInductionViewModel : ESInductionViewModel
    {
    }

    public class EditESInductionViewModel : ESInductionViewModel
    {
        [Required]
        public new int Id { get; set; }
    }
}
