using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class EYDController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EYDController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // EYD Portfolio - comprehensive professional development workspace
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser?.Role != "EYD")
            {
                return Unauthorized();
            }

            var viewModel = new EYDPortfolioViewModel
            {
                UserName = currentUser.DisplayName ?? currentUser.UserName,
                AssignedScheme = currentUser.Scheme?.Name ?? "No Scheme Assigned",
                AssignedArea = currentUser.Scheme?.Area?.Name ?? "No Area Assigned",
                // Portfolio section groups - organized by functionality
                PortfolioSectionGroups = GetPortfolioSectionGroups()
            };

            return View(viewModel);
        }

        private List<PortfolioSectionGroup> GetPortfolioSectionGroups()
        {
            return new List<PortfolioSectionGroup>
            {
                new PortfolioSectionGroup 
                { 
                    Id = "induction-group", 
                    Title = "Induction & Orientation", 
                    Icon = "fas fa-user-plus",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "es-induction", Title = "Educational Supervisor Induction Meeting", Action = "ESInduction" },
                        new PortfolioSection { Id = "induction-checklist", Title = "Induction Checklist", Action = "InductionChecklist" }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "sle-group", 
                    Title = "Supervised Learning Events (SLE)", 
                    Icon = "fas fa-chalkboard-teacher",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "sle", Title = "All SLE Activities", Action = "SLE" }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "learning-group", 
                    Title = "Learning & Development", 
                    Icon = "fas fa-graduation-cap",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "reflection", Title = "Reflection", Action = "Reflection" },
                        new PortfolioSection { Id = "protected-learning", Title = "Protected Learning Time", Action = "ProtectedLearning" },
                        new PortfolioSection { Id = "learning-needs", Title = "Learning/Development Needs", Action = "LearningNeeds" }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "uploads-group", 
                    Title = "Document Uploads", 
                    Icon = "fas fa-cloud-upload-alt",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "eyd-uploads", Title = "EYD Uploads", Action = "EYDUploads" },
                        new PortfolioSection { Id = "es-uploads", Title = "ES Uploads", Action = "ESUploads" },
                        new PortfolioSection { Id = "tpd-uploads", Title = "TPD Uploads", Action = "TPDUploads" }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "logs-group", 
                    Title = "Clinical Logs & Activities", 
                    Icon = "fas fa-clipboard-list",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "clinical-log", Title = "Clinical Experience Log", Action = "ClinicalLog" },
                        new PortfolioSection { Id = "epa", Title = "Entrustable Professional Activity L", Action = "EPA" },
                        new PortfolioSection { Id = "significant-events", Title = "Significant Event Log", Action = "SignificantEvents" }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "feedback-group", 
                    Title = "Feedback & Assessment", 
                    Icon = "fas fa-comments",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "patient-satisfaction", Title = "Patient Satisfaction Questionnaire", Action = "PatientSatisfaction" },
                        new PortfolioSection { Id = "multi-source-feedback", Title = "Multi Source Feedback Questionnaire", Action = "MultiSourceFeedback" }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "quality-group", 
                    Title = "Quality Improvement", 
                    Icon = "fas fa-chart-line",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "quality-improvement", Title = "Quality Improvement Projects", Action = "QualityImprovement" }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "reviews-group", 
                    Title = "Progress Reviews", 
                    Icon = "fas fa-tasks",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "adhoc-es-report", Title = "Ad hoc ES Report", Action = "AdHocESReport" },
                        new PortfolioSection { Id = "interim-review", Title = "Interim Review of Competence Progression", Action = "InterimReview" },
                        new PortfolioSection { Id = "final-review", Title = "Final Review of Competence Progression", Action = "FinalReview" }
                    }
                }
            };
        }

        [HttpPost]
        public async Task<IActionResult> UploadCertificate(IFormFile certificateFile, string schemeName)
        {
            if (certificateFile == null || certificateFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return RedirectToAction("Dashboard");
            }

            // Here you would implement file upload logic
            // For now, just simulate successful upload
            TempData["SuccessMessage"] = $"Certificate for {schemeName} uploaded successfully.";
            
            return RedirectToAction("Dashboard");
        }

        // Placeholder actions for portfolio sections
        public IActionResult ESInduction() => View("PlaceholderPage", new { Title = "Educational Supervisor Induction Meeting" });
        public IActionResult InductionChecklist() => View("PlaceholderPage", new { Title = "Induction Checklist" });
        public IActionResult SLE() => View("PlaceholderPage", new { Title = "Supervised Learning Events (SLE)" });
        public IActionResult QualityImprovement() => View("PlaceholderPage", new { Title = "Quality Improvement" });
        public IActionResult Reflection() => View("PlaceholderPage", new { Title = "Reflection" });
        public IActionResult ProtectedLearning() => View("PlaceholderPage", new { Title = "Protected Learning Time" });
        public IActionResult LearningNeeds() => View("PlaceholderPage", new { Title = "Learning/Development Needs" });
        public IActionResult EYDUploads() => View("PlaceholderPage", new { Title = "EYD Uploads" });
        public IActionResult ESUploads() => View("PlaceholderPage", new { Title = "ES Uploads" });
        public IActionResult TPDUploads() => View("PlaceholderPage", new { Title = "TPD Uploads" });
        public IActionResult ClinicalLog() => View("PlaceholderPage", new { Title = "Clinical Experience Log" });
        public IActionResult EPA() => View("PlaceholderPage", new { Title = "Entrustable Professional Activity L" });
        public IActionResult SignificantEvents() => View("PlaceholderPage", new { Title = "Significant Event Log" });
        public IActionResult PatientSatisfaction() => View("PlaceholderPage", new { Title = "Patient Satisfaction Questionnaire" });
        public IActionResult MultiSourceFeedback() => View("PlaceholderPage", new { Title = "Multi Source Feedback Questionnaire" });
        public IActionResult AdHocESReport() => View("PlaceholderPage", new { Title = "Ad hoc ES Report" });
        public IActionResult InterimReview() => View("PlaceholderPage", new { Title = "Interim Review of Competence Progression" });
        public IActionResult FinalReview() => View("PlaceholderPage", new { Title = "Final Review of Competence Progression" });
    }

    public class EYDPortfolioViewModel
    {
        public string UserName { get; set; } = "";
        public string AssignedScheme { get; set; } = "";
        public string AssignedArea { get; set; } = "";
        public List<PortfolioSectionGroup> PortfolioSectionGroups { get; set; } = new List<PortfolioSectionGroup>();
    }

    public class PortfolioSectionGroup
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Icon { get; set; } = "fas fa-folder";
        public List<PortfolioSection> Sections { get; set; } = new List<PortfolioSection>();
        public int TotalItems => Sections.Sum(s => s.TotalCount ?? 0);
        public int CompletedItems => Sections.Sum(s => s.CompletedCount ?? 0);
    }

    public class PortfolioSection
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Action { get; set; } = "";
        public int? CompletedCount { get; set; }
        public int? TotalCount { get; set; }
        public string Status { get; set; } = "not-started"; // not-started, in-progress, complete
    }
}
