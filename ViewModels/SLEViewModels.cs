using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class CreateSLEViewModel
    {
        public string SLEType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(7);
        
        // Location fields - different per SLE type
        public string Location { get; set; } = string.Empty; // For CBD, DOPS, Simulated DOPS, Mini-CEX
        public string Setting { get; set; } = string.Empty; // For DENTL
        public string Audience { get; set; } = string.Empty; // For DtCT
        public string AudienceSetting { get; set; } = string.Empty; // For DtCT combined
        
        // EPA Selection - validation depends on SLE type
        public List<int> SelectedEPAIds { get; set; } = new List<int>();
        
        // Assessor Selection
        public bool IsInternalAssessor { get; set; } = true;
        public string? AssessorUserId { get; set; }
        public string? ExternalAssessorName { get; set; }
        public string? ExternalAssessorEmail { get; set; }
        public string? ExternalAssessorInstitution { get; set; }
        
        // Available options for dropdowns
        public List<(string Code, string Name)> AvailableSLETypes { get; set; } = new List<(string Code, string Name)>();
        public List<ApplicationUser> AvailableAssessors { get; set; } = new List<ApplicationUser>(); // ES and TPD users in area
        public ApplicationUser? AssignedES { get; set; } // The EYD's assigned ES for quick selection
        
        // Helper method to get EPA validation requirements
        public int GetMaxEPASelection()
        {
            var singleEPATypes = new[] { SLETypes.MiniCEX, SLETypes.DOPS, SLETypes.DOPSSim };
            return singleEPATypes.Contains(SLEType) ? 1 : 2;
        }
        
        public bool RequiresSingleEPA()
        {
            var singleEPATypes = new[] { SLETypes.MiniCEX, SLETypes.DOPS, SLETypes.DOPSSim };
            return singleEPATypes.Contains(SLEType);
        }
    }
    
    public class SLEListViewModel
    {
        public string SLEType { get; set; } = string.Empty;
        public string SLETypeName { get; set; } = string.Empty;
        public List<SLESummaryItem> SLEs { get; set; } = new List<SLESummaryItem>();
        public string UserName { get; set; } = string.Empty;
        public bool CanCreateSLE { get; set; } = false; // Added this property
    }
    
    public class SLESummaryItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SLEType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string AssessorName { get; set; } = string.Empty;
        public bool IsInternalAssessor { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsAssessmentCompleted { get; set; }
        public List<string> LinkedEPAs { get; set; } = new List<string>();
    }
    
    public class SLEDetailViewModel
    {
        public SLE SLE { get; set; } = new SLE();
        public bool CanEdit { get; set; } = false;
        public bool CanAssess { get; set; } = false;
        public bool IsOwner { get; set; } = false;
        public List<EPA> LinkedEPAs { get; set; } = new List<EPA>();
        public string AssessorDisplayName { get; set; } = string.Empty;
        public string ExternalAssessmentUrl { get; set; } = string.Empty;
    }
    
    public class SLEAssessmentViewModel
    {
        public int SLEId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SLEType { get; set; } = string.Empty;
        public string EYDName { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string LearningObjectives { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Assessment Form
        public string BehaviourFeedback { get; set; } = string.Empty;
        public string AgreedAction { get; set; } = string.Empty;
        public string AssessorPosition { get; set; } = string.Empty;
        public int AssessmentRating { get; set; } = 3;
        
        // For external assessors - limited view
        public bool IsExternalAssessor { get; set; } = false;
        public string ExternalAccessToken { get; set; } = string.Empty;
        
        public List<EPA> LinkedEPAs { get; set; } = new List<EPA>();
    }
    
    public class InviteAssessorViewModel
    {
        public int SLEId { get; set; }
        public string SLETitle { get; set; } = string.Empty;
        public bool IsInternalAssessor { get; set; } = true;
        
        // Internal Assessor Options
        public string? SelectedAssessorUserId { get; set; }
        public List<ApplicationUser> AvailableESUsers { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> AvailableTPDUsers { get; set; } = new List<ApplicationUser>();
        
        // External Assessor Details
        public string? ExternalAssessorName { get; set; }
        public string? ExternalAssessorEmail { get; set; }
        public string? ExternalAssessorInstitution { get; set; }
    }
    
    public class EYDReflectionViewModel
    {
        public int SLEId { get; set; }
        public string SLETitle { get; set; } = string.Empty;
        public string BehaviourFeedback { get; set; } = string.Empty;
        public string AgreedAction { get; set; } = string.Empty;
        public string AssessorPosition { get; set; } = string.Empty;
        public int AssessmentRating { get; set; }
        public string AssessorName { get; set; } = string.Empty;
        public DateTime AssessmentCompletedAt { get; set; }
        
        // Reflection Form
        public string ReflectionNotes { get; set; } = string.Empty;
        
        public List<EPA> LinkedEPAs { get; set; } = new List<EPA>();
    }
}
