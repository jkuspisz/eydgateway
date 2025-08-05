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

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get current user and redirect to their personal portfolio
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            // Allow both EYD and ES users to access dashboard functionality
            if (currentUser?.Role != "EYD" && currentUser?.Role != "ES")
            {
                return Unauthorized();
            }

            // Redirect to user-specific portfolio (this will use their own ID)
            return RedirectToAction("Portfolio", new { id = currentUser.Id });
        }

        public async Task<IActionResult> Portfolio(string? id = null)
        {
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            // DEBUG: Log what's happening
            Console.WriteLine($"DEBUG Portfolio: Current User = {currentUser?.UserName} (Role: {currentUser?.Role})");
            Console.WriteLine($"DEBUG Portfolio: Requested id = '{id}'");
            Console.WriteLine($"DEBUG Portfolio: id is null or empty: {string.IsNullOrEmpty(id)}");
            
            // Check if id is coming from query parameters
            if (string.IsNullOrEmpty(id) && Request.Query.ContainsKey("userId"))
            {
                id = Request.Query["userId"];
                Console.WriteLine($"DEBUG Portfolio: Retrieved id from query: '{id}'");
            }

            // Allow EYD users and ES users to access portfolios
            if (currentUser?.Role != "EYD" && currentUser?.Role != "ES")
            {
                return Unauthorized();
            }

            // If no id provided, use current user's ID
            if (string.IsNullOrEmpty(id))
            {
                id = currentUser.Id;
            }

            // Security check: EYD users can only access their own portfolio
            // ES users can access portfolios of EYDs they supervise
            if (currentUser.Role == "EYD" && id != currentUser.Id)
            {
                return Forbid("You can only access your own portfolio.");
            }
            else if (currentUser.Role == "ES" && id != currentUser.Id)
            {
                // Check if the ES user supervises this EYD
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == id && 
                             assignment.IsActive);
                
                if (!isAssigned)
                {
                    return Forbid("You can only view portfolios of EYD users assigned to you.");
                }
            }

            // Get portfolio user data
            var portfolioUser = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.Id == id);

            Console.WriteLine($"DEBUG Portfolio: Target user = {portfolioUser?.UserName} (ID: {portfolioUser?.Id})");

            if (portfolioUser == null)
            {
                Console.WriteLine($"DEBUG Portfolio: Portfolio user not found for id: {id}");
                return NotFound("Portfolio user not found.");
            }

            // Get SLE completion statistics for this user
            var sleStats = await GetSLECompletionStats(portfolioUser.Id);

            var viewModel = new EYDPortfolioViewModel
            {
                UserId = portfolioUser.Id,
                UserName = portfolioUser.DisplayName ?? portfolioUser.UserName ?? "Unknown User",
                AssignedScheme = portfolioUser.Scheme?.Name ?? "No Scheme Assigned",
                AssignedArea = portfolioUser.Scheme?.Area?.Name ?? "No Area Assigned",
                SLECompletionStats = sleStats,
                // Portfolio section groups - organized by functionality
                PortfolioSectionGroups = await GetPortfolioSectionGroups(portfolioUser.Id)
            };

            Console.WriteLine($"DEBUG Portfolio: Created ViewModel for {viewModel.UserName} (UserID: {viewModel.UserId})");
            Console.WriteLine($"DEBUG Portfolio: Scheme: {viewModel.AssignedScheme}, Area: {viewModel.AssignedArea}");

            return View(viewModel);
        }

        private async Task<List<PortfolioSectionGroup>> GetPortfolioSectionGroups(string id)
        {
            // Get count data for portfolio sections that actually exist
            var reflectionTotal = await _context.Reflections.CountAsync(r => r.UserId == id);
            var reflectionComplete = await _context.Reflections.CountAsync(r => r.UserId == id && !r.IsLocked);
            
            // SLE counters (this is what's working well already)
            var sleTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == id);
            var sleComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.Status == "ReflectionCompleted");
            
            // Individual SLE type counters
            var cbdTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "CBD");
            var cbdComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "CBD" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
            
            var dopsTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DOPS");
            var dopsComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DOPS" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
            
            var miniCexTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "MiniCEX");
            var miniCexComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "MiniCEX" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
            
            var dopsSimTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DOPSSim");
            var dopsSimComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DOPSSim" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
            
            var dtctTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DtCT");
            var dtctComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DtCT" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
            
            var dentlTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DENTL");
            var dentlComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == id && s.SLEType == "DENTL" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
            
            // PLT counters - count entries and completed (not locked) entries
            var pltTotal = await _context.ProtectedLearningTimes.CountAsync(plt => plt.UserId == id);
            var pltComplete = await _context.ProtectedLearningTimes.CountAsync(plt => plt.UserId == id && !plt.IsLocked);
            
            // Learning Needs counters - count entries and completed status
            var learningNeedTotal = await _context.LearningNeeds.CountAsync(ln => ln.UserId == id);
            var learningNeedComplete = await _context.LearningNeeds.CountAsync(ln => ln.UserId == id && ln.Status == LearningNeedStatus.Completed);
            
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
                        new PortfolioSection { Id = "sle-list", Title = "All SLEs", Controller = "SLE", Action = "Index", TotalCount = sleTotal, CompletedCount = sleComplete },
                        new PortfolioSection { Id = "sle-cbd", Title = "Case Based Discussions", Controller = "SLE", Action = "Index", Parameters = "type=CBD", TotalCount = cbdTotal, CompletedCount = cbdComplete },
                        new PortfolioSection { Id = "sle-dops", Title = "Direct Observation of Procedural Skills", Controller = "SLE", Action = "Index", Parameters = "type=DOPS", TotalCount = dopsTotal, CompletedCount = dopsComplete },
                        new PortfolioSection { Id = "sle-mini-cex", Title = "Mini-Clinical Evaluation Exercise", Controller = "SLE", Action = "Index", Parameters = "type=MiniCEX", TotalCount = miniCexTotal, CompletedCount = miniCexComplete },
                        new PortfolioSection { Id = "sle-dops-sim", Title = "DOPS Under Simulated Conditions", Controller = "SLE", Action = "Index", Parameters = "type=DOPSSim", TotalCount = dopsSimTotal, CompletedCount = dopsSimComplete },
                        new PortfolioSection { Id = "sle-dtct", Title = "Developing the Clinical Teacher", Controller = "SLE", Action = "Index", Parameters = "type=DtCT", TotalCount = dtctTotal, CompletedCount = dtctComplete },
                        new PortfolioSection { Id = "sle-dentl", Title = "Direct Evaluation of Non-Technical Learning", Controller = "SLE", Action = "Index", Parameters = "type=DENTL", TotalCount = dentlTotal, CompletedCount = dentlComplete }
                    }
                },
                new PortfolioSectionGroup 
                { 
                    Id = "learning-group", 
                    Title = "Learning & Development", 
                    Icon = "fas fa-graduation-cap",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "reflection", Title = "Reflection", Controller = "Reflection", Action = "Index", TotalCount = reflectionTotal, CompletedCount = reflectionComplete },
                        new PortfolioSection { Id = "protected-learning", Title = "Protected Learning Time", Controller = "ProtectedLearningTime", Action = "Index", TotalCount = pltTotal, CompletedCount = pltComplete },
                        new PortfolioSection { Id = "learning-needs", Title = "Learning/Development Needs", Controller = "LearningNeed", Action = "Index", TotalCount = learningNeedTotal, CompletedCount = learningNeedComplete }
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
        public IActionResult ESInduction(string? id = null) => RedirectToAction("Index", "ESInduction", new { id = id });
        public IActionResult InductionChecklist(string? id = null) => View("PlaceholderPage", new { Title = "Induction Checklist" });
        public IActionResult QualityImprovement(string? id = null) => View("PlaceholderPage", new { Title = "Quality Improvement" });
        public IActionResult Reflection(string? id = null) => RedirectToAction("Index", "Reflection", new { id = id });
        public IActionResult ProtectedLearning(string? id = null) => RedirectToAction("Index", "ProtectedLearningTime", new { id = id });
        public IActionResult LearningNeeds(string? id = null) => RedirectToAction("Index", "LearningNeed", new { id = id });
        public IActionResult EYDUploads(string? id = null) => View("PlaceholderPage", new { Title = "EYD Uploads" });
        public IActionResult ESUploads(string? id = null) => View("PlaceholderPage", new { Title = "ES Uploads" });
        public IActionResult TPDUploads(string? id = null) => View("PlaceholderPage", new { Title = "TPD Uploads" });
        public IActionResult ClinicalLog(string? id = null) => View("PlaceholderPage", new { Title = "Clinical Experience Log" });
        public async Task<IActionResult> EPA(string? id = null) 
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

                // Allow both EYD and ES users to access EPA data
                if (currentUser.Role != "EYD" && currentUser.Role != "ES")
                {
                    return Unauthorized();
                }

                // Determine target user ID
                string targetUserId;
                if (string.IsNullOrEmpty(id))
                {
                    targetUserId = currentUser.Id;
                }
                else
                {
                    targetUserId = id;
                    
                    // Security check for ES users
                    if (currentUser.Role == "ES")
                    {
                        var isAssigned = await _context.EYDESAssignments
                            .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                                     assignment.EYDUserId == targetUserId && 
                                     assignment.IsActive);
                        
                        if (!isAssigned)
                        {
                            return Forbid("You can only view EPA data for EYD users assigned to you.");
                        }
                    }
                    else if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                    {
                        return Forbid("You can only view your own EPA data.");
                    }
                }

                // Get all active EPAs
                var epas = await _epaService.GetAllActiveEPAsAsync();
                
                // Get standard activity columns
                var activityColumns = ActivityTypes.GetStandardColumns();
                
                // Create the matrix
                var matrix = new EPAActivityMatrix();
                
                // Get target user's EPA mappings from database (not current user!)
                var userEPAMappings = await _context.EPAMappings
                    .Include(m => m.EPA)
                    .Where(m => m.UserId == targetUserId)  // Use targetUserId instead of currentUser.Id
                    .GroupBy(m => new { m.EPAId, m.EntityType })
                    .Select(g => new
                    {
                        EPAId = g.Key.EPAId,
                        EntityType = g.Key.EntityType,
                        Count = g.Count(),
                        LatestDate = g.Max(m => m.CreatedAt)
                    })
                    .ToListAsync();

                // Get target user's name for display
                var targetUser = await _context.Users.FindAsync(targetUserId);
                var displayName = targetUser?.DisplayName ?? targetUser?.UserName ?? "Unknown User";

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
                    UserName = displayName,  // Use target user's name, not current user's name
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
        
        public IActionResult SignificantEvents(string? id = null) => View("PlaceholderPage", new { Title = "Significant Event Log" });
        public IActionResult PatientSatisfaction(string? id = null) => View("PlaceholderPage", new { Title = "Patient Satisfaction Questionnaire" });
        public IActionResult MultiSourceFeedback(string? id = null) => View("PlaceholderPage", new { Title = "Multi Source Feedback Questionnaire" });
        public IActionResult AdHocESReport(string? id = null) => View("PlaceholderPage", new { Title = "Ad hoc ES Report" });
        public IActionResult InterimReview(string? id = null) => View("PlaceholderPage", new { Title = "Interim Review of Competence Progression" });
        public IActionResult FinalReview(string? id = null) => View("PlaceholderPage", new { Title = "Final Review of Competence Progression" });
        
        private async Task<List<SLECompletionStat>> GetSLECompletionStats(string userId)
        {
            var sleTypes = new[] { "CBD", "DOPS", "DOPSSim", "MiniCEX", "DtCT", "DENTL" };
            var stats = new List<SLECompletionStat>();
            
            foreach (var sleType in sleTypes)
            {
                var total = await _context.SLEs.CountAsync(s => s.EYDUserId == userId && s.SLEType == sleType);
                var completed = await _context.SLEs.CountAsync(s => s.EYDUserId == userId && s.SLEType == sleType && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                
                stats.Add(new SLECompletionStat
                {
                    SLEType = sleType,
                    SLETypeName = GetSLETypeName(sleType),
                    Completed = completed,
                    Total = total
                });
            }
            
            return stats;
        }
        
        private string GetSLETypeName(string sleType)
        {
            return sleType switch
            {
                "CBD" => "Case-Based Discussion",
                "DOPS" => "Direct Observation of Procedural Skills", 
                "DOPSSim" => "DOPS Under Simulated Conditions",
                "MiniCEX" => "Mini Clinical Evaluation Exercise",
                "DtCT" => "Developing the Clinical Teacher",
                "DENTL" => "Direct Evaluation of Non-Technical Learning",
                _ => sleType
            };
        }
    }

    public class EYDPortfolioViewModel
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string AssignedScheme { get; set; } = "";
        public string AssignedArea { get; set; } = "";
        public List<SLECompletionStat> SLECompletionStats { get; set; } = new List<SLECompletionStat>();
        public List<PortfolioSectionGroup> PortfolioSectionGroups { get; set; } = new List<PortfolioSectionGroup>();
    }
    
    public class SLECompletionStat
    {
        public string SLEType { get; set; } = "";
        public string SLETypeName { get; set; } = "";
        public int Completed { get; set; }
        public int Total { get; set; }
        public string ProgressPercentage => Total > 0 ? $"{(Completed * 100 / Total):F0}%" : "0%";
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
