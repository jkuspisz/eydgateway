using System.ComponentModel.DataAnnotations;

namespace EYDGateway.ViewModels
{
    public class CreateProtectedLearningTimeViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Format is required")]
        [MaxLength(100, ErrorMessage = "Format cannot exceed 100 characters")]
        public string Format { get; set; } = string.Empty;

        [Required(ErrorMessage = "Length of Protected Learning Time is required")]
        [MaxLength(100, ErrorMessage = "Length cannot exceed 100 characters")]
        public string LengthOfPLT { get; set; } = string.Empty;

        [Required(ErrorMessage = "When did it happen and who led on it is required")]
        [MaxLength(500, ErrorMessage = "This field cannot exceed 500 characters")]
        public string WhenAndWhoLed { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brief outline of learning achieved is required")]
        public string BriefOutlineOfLearning { get; set; } = string.Empty;

        // EPA Selection - validation requires minimum 2 EPAs for PLT
        [Required(ErrorMessage = "Please select at least 2 EPAs")]
        [MinLength(2, ErrorMessage = "Please select at least 2 EPAs")]
        public List<int> SelectedEPAIds { get; set; } = new List<int>();
    }

    public class EditProtectedLearningTimeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Format is required")]
        [MaxLength(100, ErrorMessage = "Format cannot exceed 100 characters")]
        public string Format { get; set; } = string.Empty;

        [Required(ErrorMessage = "Length of Protected Learning Time is required")]
        [MaxLength(100, ErrorMessage = "Length cannot exceed 100 characters")]
        public string LengthOfPLT { get; set; } = string.Empty;

        [Required(ErrorMessage = "When did it happen and who led on it is required")]
        [MaxLength(500, ErrorMessage = "This field cannot exceed 500 characters")]
        public string WhenAndWhoLed { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brief outline of learning achieved is required")]
        public string BriefOutlineOfLearning { get; set; } = string.Empty;

        // EPA Selection - validation requires minimum 2 EPAs for PLT
        [Required(ErrorMessage = "Please select at least 2 EPAs")]
        [MinLength(2, ErrorMessage = "Please select at least 2 EPAs")]
        public List<int> SelectedEPAIds { get; set; } = new List<int>();

        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
