using System.ComponentModel.DataAnnotations;

namespace EYDGateway.Models
{
    public class ESInduction
    {
        public int Id { get; set; }

        [Required]
        public string EYDUserId { get; set; } = string.Empty;
        public ApplicationUser? EYDUser { get; set; }

        [Required]
        public string ESUserId { get; set; } = string.Empty;
        public ApplicationUser? ESUser { get; set; }

        [Display(Name = "Have you read the Educational Transition Document and agreed a PDP for this placement?")]
        [Required(ErrorMessage = "Please indicate whether you have read the Educational Transition Document and agreed a PDP.")]
        public bool HasReadTransitionDocumentAndAgreedPDP { get; set; }

        [Display(Name = "Comments and notes as discussed and agreed during the meeting")]
        [Required(ErrorMessage = "Please provide comments or notes from the meeting.")]
        public string MeetingNotesAndComments { get; set; } = string.Empty;

        [Display(Name = "Brief description of the placement and details")]
        [Required(ErrorMessage = "Please provide a description of the placement.")]
        public string PlacementDescription { get; set; } = string.Empty;

        [Display(Name = "Meeting Date")]
        [DataType(DataType.Date)]
        public DateTime? MeetingDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Completed")]
        public bool IsCompleted { get; set; } = false;

        [Display(Name = "Completion Date")]
        public DateTime? CompletedAt { get; set; }
    }
}
