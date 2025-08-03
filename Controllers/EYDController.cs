using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.Services;
using EYDGateway.ViewModels;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class EYDController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEPAService _epaService;

        public EYDController(ApplicationDbContext context, IEPAService epaService)
        {
            _context = context;
            _epaService = epaService;
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
                        new PortfolioSection { Id = "sle-create", Title = "Create SLE", Controller = "SLE", Action = "Create" },
                        new PortfolioSection { Id = "sle-list", Title = "All SLEs", Controller = "SLE", Action = "Index" },
                        new PortfolioSection { Id = "sle-cbd", Title = "Case Based Discussions", Controller = "SLE", Action = "Index", Parameters = "type=CBD" },
                        new PortfolioSection { Id = "sle-dops", Title = "Direct Observation of Procedural Skills", Controller = "SLE", Action = "Index", Parameters = "type=DOPS" },
                        new PortfolioSection { Id = "sle-mini-cex", Title = "Mini-Clinical Evaluation Exercise", Controller = "SLE", Action = "Index", Parameters = "type=MiniCEX" },
                        new PortfolioSection { Id = "sle-dops-sim", Title = "DOPS Under Simulated Conditions", Controller = "SLE", Action = "Index", Parameters = "type=DOPSSim" },
                        new PortfolioSection { Id = "sle-dct", Title = "Developing the Clinical Teacher", Controller = "SLE", Action = "Index", Parameters = "type=DCT" },
                        new PortfolioSection { Id = "sle-dentl", Title = "Direct Evaluation of Non-Technical Learning", Controller = "SLE", Action = "Index", Parameters = "type=DENTL" }
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
                        new PortfolioSection { Id = "epa", Title = "Entrustable Activity Log", Action = "EPA" },
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
        public async Task<IActionResult> EPA() 
        {
            try
            {
                // Get current user
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get all active EPAs
                var epas = await _epaService.GetAllActiveEPAsAsync();
                
                // Get standard activity columns
                var activityColumns = ActivityTypes.GetStandardColumns();
                
                // Create the matrix
                var matrix = new EPAActivityMatrix();
                
                // Get user's actual EPA mappings from database
                var userEPAMappings = await _context.EPAMappings
                    .Include(m => m.EPA)
                    .Where(m => m.UserId == currentUser.Id)
                    .GroupBy(m => new { m.EPAId, m.EntityType })
                    .Select(g => new
                    {
                        EPAId = g.Key.EPAId,
                        EntityType = g.Key.EntityType,
                        Count = g.Count(),
                        LatestDate = g.Max(m => m.CreatedAt)
                    })
                    .ToListAsync();

                // Populate matrix with real user data
                foreach (var epa in epas)
                {
                    foreach (var column in activityColumns)
                    {
                        // Get actual user data for this EPA and activity type
                        var userMapping = userEPAMappings
                            .FirstOrDefault(m => m.EPAId == epa.Id && m.EntityType == column.EntityType);

                        var count = userMapping?.Count ?? 0;
                        var activities = new List<ActivitySummary>();
                        
                        // Create activity summaries based on actual data
                        if (count > 0)
                        {
                            activities.Add(new ActivitySummary
                            {
                                EntityId = epa.Id,
                                Title = $"{column.DisplayName} for {epa.Title}",
                                CreatedDate = userMapping?.LatestDate ?? DateTime.Now,
                                ActionUrl = "#", // You can link to specific activity details here
                                EntityType = column.EntityType
                            });
                        }

                        var cell = new EPAActivityCell
                        {
                            Count = count,
                            Activities = activities,
                            LatestDate = userMapping?.LatestDate,
                            IntensityClass = GetIntensityClass(count)
                        };

                        matrix.SetCell(epa.Id, column.EntityType, cell);
                        column.TotalCount += count; // Update column totals
                    }
                }

                // Create summary based on real user data
                var totalActivities = userEPAMappings.Sum(m => m.Count);
                var epasWithActivity = userEPAMappings.Select(m => m.EPAId).Distinct().Count();
                
                var summary = new EPAProgressSummary
                {
                    TotalActivities = totalActivities,
                    TotalEPAMappings = userEPAMappings.Count,
                    EPAsWithActivity = epasWithActivity,
                    EPAsNotStarted = epas.Count - epasWithActivity,
                    ActivityTypeTotals = activityColumns.ToDictionary(c => c.DisplayName, c => c.TotalCount)
                };

                // Set most/least active EPAs based on real data
                if (epas.Any() && userEPAMappings.Any())
                {
                    var epaActivityCounts = epas.Select(epa => new
                    {
                        EPA = epa,
                        TotalCount = userEPAMappings.Where(m => m.EPAId == epa.Id).Sum(m => m.Count)
                    }).Where(x => x.TotalCount > 0).ToList();

                    if (epaActivityCounts.Any())
                    {
                        var mostActive = epaActivityCounts.OrderByDescending(x => x.TotalCount).FirstOrDefault();
                        var leastActive = epaActivityCounts.OrderBy(x => x.TotalCount).FirstOrDefault();

                        summary.MostActiveEPA = mostActive?.EPA.Title ?? "";
                        summary.LeastActiveEPA = leastActive?.EPA.Title ?? "";
                    }
                }

                var viewModel = new EPALogViewModel
                {
                    EPAs = epas,
                    ActivityColumns = activityColumns,
                    Matrix = matrix,
                    UserName = currentUser.DisplayName ?? currentUser.UserName ?? "Unknown User",
                    LastActivity = userEPAMappings.Any() ? userEPAMappings.Max(m => m.LatestDate) : null,
                    Summary = summary
                };

                return View("EPAMatrix", viewModel);
            }
            catch (Exception)
            {
                // Log error and show placeholder for now
                return View("PlaceholderPage", new { Title = "EPA Activity Matrix - Error Loading Data" });
            }
        }

        private string GetIntensityClass(int count)
        {
            return count switch
            {
                0 => "intensity-0",
                1 => "intensity-1", 
                2 or 3 => "intensity-2",
                4 or 5 => "intensity-3",
                6 or 7 => "intensity-4",
                _ => "intensity-5"
            };
        }
        
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
        public string Controller { get; set; } = "EYD"; // Default controller
        public string Action { get; set; } = "";
        public string? Parameters { get; set; } // For passing URL parameters like "type=CBD"
        public int? CompletedCount { get; set; }
        public int? TotalCount { get; set; }
        public string Status { get; set; } = "not-started"; // not-started, in-progress, complete
    }
}
