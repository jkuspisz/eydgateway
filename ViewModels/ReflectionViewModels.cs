using System.ComponentModel.DataAnnotations;

namespace EYDGateway.ViewModels
{
    public class CreateReflectionViewModel
    {
        [Required(ErrorMessage = "Focus/Title of Reflection is required")]
        [MaxLength(200, ErrorMessage = "Focus/Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "When did it happen is required")]
        [MaxLength(500, ErrorMessage = "When did it happen cannot exceed 500 characters")]
        public string WhenDidItHappen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reasons for writing the reflection is required")]
        [MaxLength(2000, ErrorMessage = "Reasons for writing cannot exceed 2000 characters")]
        public string ReasonsForWriting { get; set; } = string.Empty;

        [Required(ErrorMessage = "Next steps is required")]
        [MaxLength(2000, ErrorMessage = "Next steps cannot exceed 2000 characters")]
        public string NextSteps { get; set; } = string.Empty;

        // EPA Selection - validation requires minimum 2 EPAs for Reflections
        [Required(ErrorMessage = "Please select at least 2 EPAs")]
        [MinLength(2, ErrorMessage = "Please select at least 2 EPAs")]
        public List<int> SelectedEPAIds { get; set; } = new List<int>();
    }

    public class EditReflectionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Focus/Title of Reflection is required")]
        [MaxLength(200, ErrorMessage = "Focus/Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "When did it happen is required")]
        [MaxLength(500, ErrorMessage = "When did it happen cannot exceed 500 characters")]
        public string WhenDidItHappen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reasons for writing the reflection is required")]
        [MaxLength(2000, ErrorMessage = "Reasons for writing cannot exceed 2000 characters")]
        public string ReasonsForWriting { get; set; } = string.Empty;

        [Required(ErrorMessage = "Next steps is required")]
        [MaxLength(2000, ErrorMessage = "Next steps cannot exceed 2000 characters")]
        public string NextSteps { get; set; } = string.Empty;

        // EPA Selection - validation requires minimum 2 EPAs for Reflections
        [Required(ErrorMessage = "Please select at least 2 EPAs")]
        [MinLength(2, ErrorMessage = "Please select at least 2 EPAs")]
        public List<int> SelectedEPAIds { get; set; } = new List<int>();

        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
