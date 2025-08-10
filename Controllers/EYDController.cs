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

            // Allow EYD, ES, TPD, Dean, and Admin users to access portfolios
            if (currentUser?.Role != "EYD" && currentUser?.Role != "ES" && 
                currentUser?.Role != "TPD" && currentUser?.Role != "Dean" && currentUser?.Role != "Admin")
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
            // TPD and Dean can view any portfolio
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
            // TPD, Dean, and Admin can access any portfolio

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

            // Get IRCP status for this user
            var ircpStatus = GetIRCPStatus(portfolioUser.Id);
            
            // Get FRCP status for this user
            var frcpStatus = GetFRCPStatus(portfolioUser.Id);
            
            // Get PSQ status for this user
            var psqStatus = await GetPSQStatusAsync(portfolioUser.Id);
            
            // Get MSF status for this user
            var msfStatus = await GetMSFStatusAsync(portfolioUser.Id);
            
            // Get Ad Hoc ES Report status for this user
            var adHocESReportStatuses = await GetAdHocESReportStatusesAsync(portfolioUser.Id);

            var viewModel = new EYDPortfolioViewModel
            {
                UserId = portfolioUser.Id,
                UserName = portfolioUser.DisplayName ?? portfolioUser.UserName ?? "Unknown User",
                AssignedScheme = portfolioUser.Scheme?.Name ?? "No Scheme Assigned",
                AssignedArea = portfolioUser.Scheme?.Area?.Name ?? "No Area Assigned",
                SLECompletionStats = sleStats,
                // Portfolio section groups - organized by functionality
                PortfolioSectionGroups = await GetPortfolioSectionGroups(portfolioUser.Id),
                // IRCP Status
                IRCPESStatus = ircpStatus.ESStatus,
                IRCPEYDStatus = ircpStatus.EYDStatus,
                IRCPPanelStatus = ircpStatus.PanelStatus,
                // FRCP Status
                FRCPESStatus = frcpStatus.ESStatus,
                FRCPEYDStatus = frcpStatus.EYDStatus,
                FRCPPanelStatus = frcpStatus.PanelStatus,
                // PSQ Status
                PSQStatus = psqStatus,
                // MSF Status
                MSFStatus = msfStatus,
                // Ad Hoc ES Report Status
                AdHocESReportESStatus = adHocESReportStatuses.ESStatus,
                AdHocESReportEYDStatus = adHocESReportStatuses.EYDStatus
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
            
            // Clinical Log counters - count entries and completed entries
            var clinicalLogTotal = await _context.ClinicalLogs.CountAsync(cl => cl.EYDUserId == id);
            var clinicalLogComplete = await _context.ClinicalLogs.CountAsync(cl => cl.EYDUserId == id && cl.IsCompleted);
            
            // Learning Needs counters - count entries and completed status
            var learningNeedTotal = await _context.LearningNeeds.CountAsync(ln => ln.UserId == id);
            var learningNeedComplete = await _context.LearningNeeds.CountAsync(ln => ln.UserId == id && ln.Status == LearningNeedStatus.Completed);
            
            // Significant Event counters - count entries and locked status
            var significantEventTotal = await _context.SignificantEvents.CountAsync(se => se.UserId == id);
            var significantEventComplete = await _context.SignificantEvents.CountAsync(se => se.UserId == id && se.IsLocked);
            
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
                    Id = "logs-group", 
                    Title = "Clinical Logs & Activities", 
                    Icon = "fas fa-clipboard-list",
                    Sections = new List<PortfolioSection>
                    {
                        new PortfolioSection { Id = "clinical-log", Title = "Monthly Clinical Log", Controller = "ClinicalLog", Action = "Index", TotalCount = clinicalLogTotal, CompletedCount = clinicalLogComplete },
                        new PortfolioSection { Id = "epa", Title = "Entrustable Activity Log", Action = "EPA" },
                        new PortfolioSection { Id = "significant-events", Title = "Significant Event Log", Controller = "SignificantEvent", Action = "Index", TotalCount = significantEventTotal, CompletedCount = significantEventComplete }
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

                // Allow EYD, ES, TPD, Dean, and Admin users to access EPA data
                if (currentUser.Role != "EYD" && currentUser.Role != "ES" && 
                    currentUser.Role != "TPD" && currentUser.Role != "Dean" && currentUser.Role != "Admin")
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
                    
                    // Security check for different user roles
                    if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                    {
                        return Forbid("You can only view your own EPA data.");
                    }
                    else if (currentUser.Role == "ES")
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
                    else if ((currentUser.Role == "TPD" || currentUser.Role == "Dean") && targetUserId != currentUser.Id)
                    {
                        // TPD and Dean can view EPA data for users in their area/scheme
                        var epaTargetUser = await _context.Users.FindAsync(targetUserId);
                        if (epaTargetUser == null || 
                            (currentUser.Role == "TPD" && epaTargetUser.SchemeId != currentUser.SchemeId) ||
                            (currentUser.Role == "Dean" && epaTargetUser.AreaId != currentUser.AreaId))
                        {
                            return Forbid("You can only view EPA data for users in your area/scheme.");
                        }
                    }
                    // Admin can access any EPA data
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
        
        public async Task<IActionResult> PatientSatisfaction(string? id = null)
        {
            try
            {
                // Get current user
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Allow EYD, ES, TPD, and Dean users to access PSQ data
                if (currentUser.Role != "EYD" && currentUser.Role != "ES" && currentUser.Role != "TPD" && currentUser.Role != "Dean")
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
                    
                    // Security check for different user roles
                    if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                    {
                        return Forbid("You can only view your own PSQ data.");
                    }
                    else if (currentUser.Role == "ES")
                    {
                        // Check if the ES user supervises this EYD
                        var isAssigned = await _context.EYDESAssignments
                            .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                                     assignment.EYDUserId == targetUserId && 
                                     assignment.IsActive);
                        
                        if (!isAssigned)
                        {
                            return Forbid("You can only view PSQ data for EYD users assigned to you.");
                        }
                    }
                    else if (currentUser.Role == "TPD" || currentUser.Role == "Dean")
                    {
                        // TPD and Dean can view PSQs for users in their area/scheme
                        var portfolioUser = await _context.Users.FindAsync(targetUserId);
                        if (portfolioUser == null || 
                            (currentUser.Role == "TPD" && portfolioUser.SchemeId != currentUser.SchemeId) ||
                            (currentUser.Role == "Dean" && portfolioUser.AreaId != currentUser.AreaId))
                        {
                            return Forbid("You can only view PSQ data for users in your area/scheme.");
                        }
                    }
                }

                // Get target user's name for display
                var targetUser = await _context.Users.FindAsync(targetUserId);
                if (targetUser == null)
                {
                    return NotFound("User not found.");
                }

                // Get or create PSQ questionnaire for this user
                var questionnaire = await _context.PSQQuestionnaires
                    .FirstOrDefaultAsync(q => q.PerformerId == targetUserId && q.IsActive);

                if (questionnaire == null && currentUser.Role == "EYD" && targetUserId == currentUser.Id)
                {
                    // Only EYD users can create their own PSQ questionnaires
                    questionnaire = new PSQQuestionnaire
                    {
                        PerformerId = targetUserId,
                        Title = $"PSQ Assessment for {targetUser.DisplayName ?? targetUser.UserName}",
                        UniqueCode = GenerateUniqueCode(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.PSQQuestionnaires.Add(questionnaire);
                    await _context.SaveChangesAsync();
                }

                if (questionnaire == null)
                {
                    // For supervisors viewing EYD users who haven't created a PSQ yet
                    return View("PSQNotCreated", new { TargetUserName = targetUser.DisplayName ?? targetUser.UserName });
                }

                // Get PSQ results
                var results = await GetPSQResults(questionnaire.Id, targetUser);
                
                // Set ViewBag for QR code and URL generation
                ViewBag.UniqueCode = questionnaire.UniqueCode;
                ViewBag.PerformerId = targetUserId;
                
                return View("PatientSatisfactionResults", results);
            }
            catch (Exception)
            {
                return View("PlaceholderPage", new { Title = "Patient Satisfaction Questionnaire - Error Loading Data" });
            }
        }

        public async Task<IActionResult> MultiSourceFeedback(string? id = null)
        {
            try
            {
                // Get current user
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Allow EYD, ES, TPD, and Dean users to access MSF data
                if (currentUser.Role != "EYD" && currentUser.Role != "ES" && currentUser.Role != "TPD" && currentUser.Role != "Dean")
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
                    
                    // Security check for different user roles
                    if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                    {
                        return Forbid("You can only view your own MSF data.");
                    }
                    else if (currentUser.Role == "ES")
                    {
                        // Check if the ES user supervises this EYD
                        var isAssigned = await _context.EYDESAssignments
                            .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                                     assignment.EYDUserId == targetUserId && 
                                     assignment.IsActive);
                        
                        if (!isAssigned)
                        {
                            return Forbid("You can only view MSF data for EYD users assigned to you.");
                        }
                    }
                    else if (currentUser.Role == "TPD" || currentUser.Role == "Dean")
                    {
                        // TPD and Dean can view MSFs for users in their area/scheme
                        var portfolioUser = await _context.Users.FindAsync(targetUserId);
                        if (portfolioUser == null || 
                            (currentUser.Role == "TPD" && portfolioUser.SchemeId != currentUser.SchemeId) ||
                            (currentUser.Role == "Dean" && portfolioUser.AreaId != currentUser.AreaId))
                        {
                            return Forbid("You can only view MSF data for users in your area/scheme.");
                        }
                    }
                }

                // Get target user's name for display
                var targetUser = await _context.Users.FindAsync(targetUserId);
                if (targetUser == null)
                {
                    return NotFound("User not found.");
                }

                // Get or create MSF questionnaire for this user
                var questionnaire = await _context.MSFQuestionnaires
                    .FirstOrDefaultAsync(q => q.PerformerId == targetUserId && q.IsActive);

                if (questionnaire == null && currentUser.Role == "EYD" && targetUserId == currentUser.Id)
                {
                    // Only EYD users can create their own MSF questionnaires
                    questionnaire = new MSFQuestionnaire
                    {
                        PerformerId = targetUserId,
                        Title = $"MSF Assessment for {targetUser.DisplayName ?? targetUser.UserName}",
                        UniqueCode = GenerateUniqueCode(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.MSFQuestionnaires.Add(questionnaire);
                    await _context.SaveChangesAsync();
                }

                if (questionnaire == null)
                {
                    // For supervisors viewing EYD users who haven't created a MSF yet
                    return View("MSFNotCreated", new { TargetUserName = targetUser.DisplayName ?? targetUser.UserName });
                }

                // Get MSF results
                var results = await GetMSFResults(questionnaire.Id, targetUser);
                
                // Set ViewBag for QR code and URL generation
                ViewBag.UniqueCode = questionnaire.UniqueCode;
                ViewBag.PerformerId = targetUserId;
                
                return View("MultiSourceFeedbackResults", results);
            }
            catch (Exception)
            {
                return View("PlaceholderPage", new { Title = "Multi Source Feedback Questionnaire - Error Loading Data" });
            }
        }
        
        public async Task<IActionResult> AdHocESReport(string? id = null)
        {
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Allow EYD, ES, TPD, Dean, and Admin users to access Ad Hoc ES Reports
            if (currentUser.Role != "EYD" && currentUser.Role != "ES" && currentUser.Role != "TPD" && 
                currentUser.Role != "Dean" && currentUser.Role != "Admin")
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
                
                // Security check for different user roles
                if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                {
                    return Forbid("You can only view your own Ad Hoc ES Report data.");
                }
                else if (currentUser.Role == "ES")
                {
                    // Check if the ES user supervises this EYD
                    var isAssigned = await _context.EYDESAssignments
                        .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                                 assignment.EYDUserId == targetUserId && 
                                 assignment.IsActive);
                    
                    if (!isAssigned)
                    {
                        return Forbid("You can only access Ad Hoc ES Reports for EYD users assigned to you.");
                    }
                }
                else if (currentUser.Role == "TPD" || currentUser.Role == "Dean")
                {
                    // TPD and Dean can view reports for users in their area/scheme
                    var portfolioUser = await _context.Users.FindAsync(targetUserId);
                    if (portfolioUser == null || 
                        (currentUser.Role == "TPD" && portfolioUser.SchemeId != currentUser.SchemeId) ||
                        (currentUser.Role == "Dean" && portfolioUser.AreaId != currentUser.AreaId))
                    {
                        return Forbid("You can only view Ad Hoc ES Report data for users in your area/scheme.");
                    }
                }
                // Admin can access any report
            }

            // Get target user details
            var targetUser = await _context.Users
                .Include(u => u.Scheme)
                .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (targetUser == null)
            {
                return NotFound("User not found.");
            }

            // Get or create Ad Hoc ES Report record
            var report = await _context.AdHocESReports
                .Include(r => r.EPAAssessments)
                .ThenInclude(a => a.EPA)
                .FirstOrDefaultAsync(r => r.EYDUserId == targetUserId);

            if (report == null)
            {
                report = new AdHocESReport
                {
                    EYDUserId = targetUserId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AdHocESReports.Add(report);
                await _context.SaveChangesAsync();
            }

            // Get real EPA data for the target user
            var epas = await _epaService.GetAllActiveEPAsAsync();
            var activityColumns = ActivityTypes.GetStandardColumns();
            
            // Get target user's EPA mappings from database
            var userEPAMappings = await _context.EPAMappings
                .Include(m => m.EPA)
                .Where(m => m.UserId == targetUserId)
                .GroupBy(m => new { m.EPAId, m.EntityType })
                .Select(g => new
                {
                    EPAId = g.Key.EPAId,
                    EntityType = g.Key.EntityType,
                    Count = g.Count(),
                    LatestDate = g.Max(m => m.CreatedAt)
                })
                .ToListAsync();

            // Create EPA matrix data for the view
            var epaMatrixData = epas.Select(epa => new
            {
                Id = epa.Id,
                Code = epa.Code,
                Title = epa.Title,
                Activities = activityColumns.Select(col => 
                {
                    var mapping = userEPAMappings
                        .FirstOrDefault(m => m.EPAId == epa.Id && m.EntityType == col.EntityType);
                    return mapping?.Count ?? 0;
                }).ToArray(),
                Total = userEPAMappings.Where(m => m.EPAId == epa.Id).Sum(m => m.Count)
            }).ToList();

            // Determine workflow status
            var esStatus = report.IsESCompleted ? "Completed" : (report.ESCompletedAt.HasValue ? "InProgress" : "NotStarted");
            var eydStatus = report.IsEYDCompleted ? "Completed" : (report.EYDCompletedAt.HasValue ? "InProgress" : "NotStarted");
            
            // Determine edit permissions
            bool canEditES = currentUser.Role == "ES" && !report.IsESCompleted;
            bool canEditEYD = currentUser.Role == "EYD" && targetUserId == currentUser.Id && !report.IsEYDCompleted;
            bool canUnlock = currentUser.Role == "Admin" || currentUser.Role == "TPD";

            var viewModel = new
            {
                CurrentUser = currentUser,
                TargetUser = targetUser,
                Report = report,
                EPAs = epas,
                EPAMatrixData = epaMatrixData,
                ActivityColumns = activityColumns,
                ESStatus = esStatus,
                EYDStatus = eydStatus,
                CanEditES = canEditES,
                CanEditEYD = canEditEYD,
                CanUnlock = canUnlock
            };

            return View("AdHocESReport", viewModel);
        }
        public async Task<IActionResult> InterimReview(string? id = null)
        {
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Allow EYD, ES, TPD, Dean, and Admin users to access IRCP
            if (currentUser.Role != "EYD" && currentUser.Role != "ES" && currentUser.Role != "TPD" && 
                currentUser.Role != "Dean" && currentUser.Role != "Admin")
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
                
                // Security check for different user roles
                if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                {
                    return Forbid("You can only view your own IRCP data.");
                }
                else if (currentUser.Role == "ES")
                {
                    // Check if the ES user supervises this EYD
                    var isAssigned = await _context.EYDESAssignments
                        .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                                 assignment.EYDUserId == targetUserId && 
                                 assignment.IsActive);
                    
                    if (!isAssigned)
                    {
                        return Forbid("You can only access IRCP for EYD users assigned to you.");
                    }
                }
                else if (currentUser.Role == "TPD" || currentUser.Role == "Dean")
                {
                    // TPD and Dean can view IRCPs for users in their area/scheme
                    var portfolioUser = await _context.Users.FindAsync(targetUserId);
                    if (portfolioUser == null || 
                        (currentUser.Role == "TPD" && portfolioUser.SchemeId != currentUser.SchemeId) ||
                        (currentUser.Role == "Dean" && portfolioUser.AreaId != currentUser.AreaId))
                    {
                        return Forbid("You can only view IRCP data for users in your area/scheme.");
                    }
                }
                // Admin can access any IRCP
            }

            // Get target user details
            var targetUser = await _context.Users
                .Include(u => u.Scheme)
                .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (targetUser == null)
            {
                return NotFound("User not found.");
            }

            // Get or create IRCP review record
            var ircpReview = await _context.IRCPReviews
                .Include(r => r.ESAssessments)
                .Include(r => r.EYDReflection)
                .Include(r => r.PanelReview)
                .FirstOrDefaultAsync(r => r.EYDUserId == targetUserId);

            // Get real EPA data for the target user
            var epas = await _epaService.GetAllActiveEPAsAsync();
            var activityColumns = ActivityTypes.GetStandardColumns();
            
            // Get target user's EPA mappings from database
            var userEPAMappings = await _context.EPAMappings
                .Include(m => m.EPA)
                .Where(m => m.UserId == targetUserId)
                .GroupBy(m => new { m.EPAId, m.EntityType })
                .Select(g => new
                {
                    EPAId = g.Key.EPAId,
                    EntityType = g.Key.EntityType,
                    Count = g.Count(),
                    LatestDate = g.Max(m => m.CreatedAt)
                })
                .ToListAsync();

            // Create EPA matrix data for the view
            var epaMatrixData = epas.Select(epa => new
            {
                Id = epa.Id,
                Code = epa.Code,
                Title = epa.Title,
                Activities = activityColumns.Select(col => 
                {
                    var mapping = userEPAMappings
                        .FirstOrDefault(m => m.EPAId == epa.Id && m.EntityType == col.EntityType);
                    return mapping?.Count ?? 0;
                }).ToArray(),
                Total = userEPAMappings.Where(m => m.EPAId == epa.Id).Sum(m => m.Count)
            }).ToList();

            // Check for section locks - TempData first, then database (TempData-first approach)
            bool esLocked = TempData[$"IRCP_{targetUserId}_ES_Locked"]?.ToString() == "true";
            bool eydLocked = TempData[$"IRCP_{targetUserId}_EYD_Locked"]?.ToString() == "true";
            bool panelLocked = TempData[$"IRCP_{targetUserId}_Panel_Locked"]?.ToString() == "true";
            
            // If not locked in TempData, check database
            if (!esLocked || !eydLocked || !panelLocked)
            {
                if (!esLocked)
                    esLocked = ircpReview?.ESLocked ?? false;
                if (!eydLocked)
                    eydLocked = ircpReview?.EYDLocked ?? false;
                if (!panelLocked)
                    panelLocked = ircpReview?.PanelLocked ?? false;
            }
            
            // Keep lock data for next request if using TempData
            if (TempData[$"IRCP_{targetUserId}_ES_Locked"] != null) TempData.Keep($"IRCP_{targetUserId}_ES_Locked");
            if (TempData[$"IRCP_{targetUserId}_EYD_Locked"] != null) TempData.Keep($"IRCP_{targetUserId}_EYD_Locked");
            if (TempData[$"IRCP_{targetUserId}_Panel_Locked"] != null) TempData.Keep($"IRCP_{targetUserId}_Panel_Locked");

            // Initialize status - TempData-first approach
            var esStatus = "NotStarted";
            var eydStatus = "NotStarted";
            var panelStatus = "NotStarted";

            // If locked in TempData, immediately set to Completed
            if (esLocked)
            {
                esStatus = "Completed";
            }
            if (eydLocked)
            {
                eydStatus = "Completed";
            }
            if (panelLocked)
            {
                panelStatus = "Completed";
            }

            // If not locked, check database status first, then TempData content
            if (!esLocked)
            {
                if (ircpReview != null)
                {
                    esStatus = ircpReview.ESStatus.ToString();
                }
                else if (TempData[$"IRCP_{targetUserId}_ES"] != null)
                {
                    var jsonData = TempData[$"IRCP_{targetUserId}_ES"]?.ToString();
                    if (jsonData != null)
                    {
                        var esData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                        
                        // Check if ES has confirmed their assessment (this indicates completion)
                        if (esData.ContainsKey("ESConfirmation") && esData["ESConfirmation"] == "true")
                        {
                            esStatus = "Completed";
                        }
                        else if (esData.Count > 0)
                        {
                            esStatus = "InProgress";
                        }
                    }
                    TempData.Keep($"IRCP_{targetUserId}_ES");
                }
            }

            if (!eydLocked)
            {
                if (ircpReview != null)
                {
                    eydStatus = ircpReview.EYDStatus.ToString();
                }
                else if (TempData[$"IRCP_{targetUserId}_EYD"] != null)
                {
                    var jsonData = TempData[$"IRCP_{targetUserId}_EYD"]?.ToString();
                    if (jsonData != null)
                    {
                        var eydData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                        if (eydData.Count > 0)
                        {
                            eydStatus = "InProgress";
                        }
                    }
                    TempData.Keep($"IRCP_{targetUserId}_EYD");
                }
            }

            if (!panelLocked)
            {
                if (ircpReview != null)
                {
                    panelStatus = ircpReview.PanelStatus.ToString();
                }
                else if (TempData[$"IRCP_{targetUserId}_Panel"] != null)
                {
                    var jsonData = TempData[$"IRCP_{targetUserId}_Panel"]?.ToString();
                    if (jsonData != null)
                    {
                        var panelData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                        if (panelData.Count > 0)
                        {
                            panelStatus = "InProgress";
                        }
                    }
                    TempData.Keep($"IRCP_{targetUserId}_Panel");
                }
            }

            // Determine edit permissions based on workflow status and locks
            bool canEditES = currentUser.Role == "ES" && !esLocked;
            
            bool canEditEYD = currentUser.Role == "EYD" && targetUserId == currentUser.Id && !eydLocked;
            
            // TPD/Dean can always access their Panel section regardless of ES/EYD completion
            bool canEditPanel = (currentUser.Role == "TPD" || currentUser.Role == "Dean") && !panelLocked;
            
            bool canUnlock = currentUser.Role == "Admin" || currentUser.Role == "TPD";

            // Load saved form data if it exists
            var savedESData = new Dictionary<string, string>();
            var savedEYDData = new Dictionary<string, string>();
            var savedPanelData = new Dictionary<string, string>();

            if (TempData[$"IRCP_{targetUserId}_ES"] != null)
            {
                var jsonData = TempData[$"IRCP_{targetUserId}_ES"].ToString();
                savedESData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                TempData.Keep($"IRCP_{targetUserId}_ES"); // Keep for next request
            }

            if (TempData[$"IRCP_{targetUserId}_EYD"] != null)
            {
                var jsonData = TempData[$"IRCP_{targetUserId}_EYD"].ToString();
                savedEYDData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                TempData.Keep($"IRCP_{targetUserId}_EYD"); // Keep for next request
            }

            if (TempData[$"IRCP_{targetUserId}_Panel"] != null)
            {
                var jsonData = TempData[$"IRCP_{targetUserId}_Panel"].ToString();
                savedPanelData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                TempData.Keep($"IRCP_{targetUserId}_Panel"); // Keep for next request
            }

            // Create view model
            var viewModel = new
            {
                UserId = targetUser.Id,
                UserName = targetUser.DisplayName ?? targetUser.UserName,
                CurrentUserRole = currentUser.Role,
                IsCurrentUser = targetUserId == currentUser.Id,
                
                // Edit permissions
                CanEditES = canEditES,
                CanEditEYD = canEditEYD,
                CanEditPanel = canEditPanel,
                CanUnlock = canUnlock,
                
                // Lock status
                ESLocked = esLocked,
                EYDLocked = eydLocked,
                PanelLocked = panelLocked,
                
                // Workflow status (mock for now)
                ESStatus = esStatus,
                EYDStatus = eydStatus,
                PanelStatus = panelStatus,
                
                // Real EPA matrix data
                EPAData = epaMatrixData,
                ActivityColumns = activityColumns,
                
                // Saved form data
                SavedESData = savedESData,
                SavedEYDData = savedEYDData,
                SavedPanelData = savedPanelData
            };

            return View("InterimReview", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveIRCPSection(string section, string userId, string action)
        {
            try
            {
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction("InterimReview", new { id = userId });
                }

                // For now, simulate saving by storing form data in TempData
                // This will persist the data across the redirect so user can see it was saved
                var formData = new Dictionary<string, string>();
                foreach (var key in Request.Form.Keys)
                {
                    formData[key] = Request.Form[key].ToString();
                }
                
                // Store the form data for this user and section
                var sessionKey = $"IRCP_{userId}_{section}";
                TempData[sessionKey] = System.Text.Json.JsonSerializer.Serialize(formData);

                bool isSubmit = action == "submit";

                // If submitting, lock the section
                if (isSubmit)
                {
                    var lockKey = $"IRCP_{userId}_{section}_Locked";
                    TempData[lockKey] = "true";
                    TempData.Keep(lockKey);
                }

                // Set success message
                TempData["SuccessMessage"] = isSubmit ? "Section submitted and locked successfully" : "Progress saved successfully";
                
                // Redirect back to the InterimReview page for the same user
                return RedirectToAction("InterimReview", new { id = userId });
            }
            catch (Exception ex)
            {
                // Set error message and redirect back
                TempData["ErrorMessage"] = $"Error saving section: {ex.Message}";
                return RedirectToAction("InterimReview", new { id = userId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockIRCPSection(string section, string userId)
        {
            try
            {
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction("InterimReview", new { id = userId });
                }

                // Check if user has permission to unlock (Admin, TPD, or Dean)
                if (currentUser.Role != "Admin" && currentUser.Role != "TPD" && currentUser.Role != "Dean")
                {
                    TempData["ErrorMessage"] = "You don't have permission to unlock sections";
                    return RedirectToAction("InterimReview", new { id = userId });
                }

                // Find or create the IRCP review record
                var ircpReview = await _context.IRCPReviews
                    .FirstOrDefaultAsync(r => r.EYDUserId == userId);

                if (ircpReview != null)
                {
                    // Unlock the specific section in the database
                    switch (section.ToUpper())
                    {
                        case "ES":
                            ircpReview.ESLocked = false;
                            break;
                        case "EYD":
                            ircpReview.EYDLocked = false;
                            break;
                        case "PANEL":
                            ircpReview.PanelLocked = false;
                            break;
                        default:
                            TempData["ErrorMessage"] = "Invalid section specified";
                            return RedirectToAction("InterimReview", new { id = userId });
                    }

                    ircpReview.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Also remove TempData lock for immediate effect
                var lockKey = $"IRCP_{userId}_{section}_Locked";
                TempData.Remove(lockKey);

                TempData["SuccessMessage"] = $"{section} section unlocked successfully";
                return RedirectToAction("InterimReview", new { id = userId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error unlocking section: {ex.Message}";
                return RedirectToAction("InterimReview", new { id = userId });
            }
        }

        private async Task SaveESSection(IRCPReview ircpReview, IFormCollection form, bool isSubmit)
        {
            // Clear existing ES assessments if resubmitting
            var existingAssessments = await _context.IRCPESAssessments
                .Where(a => a.IRCPReviewId == ircpReview.Id)
                .ToListAsync();
            _context.IRCPESAssessments.RemoveRange(existingAssessments);

            // Get all EPAs for validation
            var epas = await _epaService.GetAllActiveEPAsAsync();

            // Save EPA assessments
            foreach (var epa in epas)
            {
                var entrustmentKey = $"entrustment_{epa.Id}";
                var reasonKey = $"reason_{epa.Id}";

                if (form.ContainsKey(entrustmentKey))
                {
                    var assessment = new IRCPESAssessment
                    {
                        IRCPReviewId = ircpReview.Id,
                        EPACode = epa.Code,
                        EntrustmentLevel = int.TryParse(form[entrustmentKey].ToString(), out int level) ? level : null,
                        Justification = form[reasonKey].ToString(),
                        CreatedDate = DateTime.UtcNow
                    };
                    
                    _context.IRCPESAssessments.Add(assessment);
                }
            }

            // Create or update ES section
            var esSection = await _context.IRCPESSections
                .FirstOrDefaultAsync(s => s.IRCPReviewId == ircpReview.Id);

            if (esSection == null)
            {
                esSection = new IRCPESSection
                {
                    IRCPReviewId = ircpReview.Id,
                    CreatedDate = DateTime.UtcNow
                };
                _context.IRCPESSections.Add(esSection);
            }

            esSection.ConfirmAccuracy = form.ContainsKey("ConfirmAccuracy");
            esSection.LastModifiedDate = DateTime.UtcNow;

            // Update status
            if (isSubmit)
            {
                ircpReview.ESStatus = IRCPStatus.Completed;
                ircpReview.ESSubmittedDate = DateTime.UtcNow;
                ircpReview.ESLocked = true;
            }
            else
            {
                ircpReview.ESStatus = IRCPStatus.InProgress;
            }
        }

        private async Task SaveEYDSection(IRCPReview ircpReview, IFormCollection form, bool isSubmit)
        {
            // Get or create EYD reflection
            var eydReflection = await _context.IRCPEYDReflections
                .FirstOrDefaultAsync(r => r.IRCPReviewId == ircpReview.Id);

            if (eydReflection == null)
            {
                eydReflection = new IRCPEYDReflection
                {
                    IRCPReviewId = ircpReview.Id,
                    CreatedDate = DateTime.UtcNow
                };
                _context.IRCPEYDReflections.Add(eydReflection);
            }

            eydReflection.Reflection = form["EYDReflection"].ToString();
            eydReflection.LastModifiedDate = DateTime.UtcNow;

            // Update status
            if (isSubmit)
            {
                ircpReview.EYDStatus = IRCPStatus.Completed;
                ircpReview.EYDSubmittedDate = DateTime.UtcNow;
                ircpReview.EYDLocked = true;
            }
            else
            {
                ircpReview.EYDStatus = IRCPStatus.InProgress;
            }
        }

        private async Task SavePanelSection(IRCPReview ircpReview, IFormCollection form, bool isSubmit)
        {
            // Get or create panel review
            var panelReview = await _context.IRCPPanelReviews
                .FirstOrDefaultAsync(r => r.IRCPReviewId == ircpReview.Id);

            if (panelReview == null)
            {
                panelReview = new IRCPPanelReview
                {
                    IRCPReviewId = ircpReview.Id,
                    CreatedDate = DateTime.UtcNow
                };
                _context.IRCPPanelReviews.Add(panelReview);
            }

            panelReview.RecommendedOutcome = form["RecommendedOutcome"].ToString();
            panelReview.DetailedReasons = form["DetailedReasons"].ToString();
            panelReview.MitigatingCircumstances = form["MitigatingCircumstances"].ToString();
            panelReview.CompetenciesToDevelop = form["CompetenciesToDevelop"].ToString();
            panelReview.RecommendedActions = form["RecommendedActions"].ToString();
            panelReview.LastModifiedDate = DateTime.UtcNow;

            // Update status
            if (isSubmit)
            {
                ircpReview.PanelStatus = IRCPStatus.Completed;
                ircpReview.PanelSubmittedDate = DateTime.UtcNow;
                ircpReview.PanelLocked = true;
            }
            else
            {
                ircpReview.PanelStatus = IRCPStatus.InProgress;
            }
        }



        public async Task<IActionResult> FinalReview(string? id = null)
        {
            // Get current user
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            // Determine target user
            var targetUserId = id ?? currentUser.Id;
            var targetUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (targetUser == null)
            {
                return NotFound("Target user not found");
            }

            // Security checks
            bool canAccess = false;
            switch (currentUser.Role)
            {
                case "EYD":
                    canAccess = targetUserId == currentUser.Id;
                    break;
                case "ES":
                case "TPD":
                case "Dean":
                case "Admin":
                    canAccess = true;
                    break;
                default:
                    canAccess = false;
                    break;
            }

            if (!canAccess)
            {
                return Forbid("You don't have permission to view this page");
            }

            // Get real EPA data for the target user
            var epas = await _epaService.GetAllActiveEPAsAsync();
            var activityColumns = ActivityTypes.GetStandardColumns();
            
            // Get target user's EPA mappings from database
            var userEPAMappings = await _context.EPAMappings
                .Include(m => m.EPA)
                .Where(m => m.UserId == targetUserId)
                .GroupBy(m => new { m.EPAId, m.EntityType })
                .Select(g => new
                {
                    EPAId = g.Key.EPAId,
                    EntityType = g.Key.EntityType,
                    Count = g.Count(),
                    LatestDate = g.Max(m => m.CreatedAt)
                })
                .ToListAsync();

            // Create EPA matrix data for the view
            var epaMatrixData = epas.Select(epa => new
            {
                Id = epa.Id,
                Code = epa.Code,
                Title = epa.Title,
                Activities = activityColumns.Select(col => 
                {
                    var mapping = userEPAMappings
                        .FirstOrDefault(m => m.EPAId == epa.Id && m.EntityType == col.EntityType);
                    return mapping?.Count ?? 0;
                }).ToArray(),
                Total = userEPAMappings.Where(m => m.EPAId == epa.Id).Sum(m => m.Count)
            }).ToList();

            // Get or create FRCP review record
            var frcpReview = await _context.FRCPReviews
                .FirstOrDefaultAsync(r => r.EYDUserId == targetUserId);

            if (frcpReview == null)
            {
                frcpReview = new FRCPReview
                {
                    EYDUserId = targetUserId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                _context.FRCPReviews.Add(frcpReview);
                await _context.SaveChangesAsync();
            }

            // Check lock status from database first, then fall back to TempData
            bool esLocked = frcpReview.ESLocked;
            bool eydLocked = frcpReview.EYDLocked;
            bool panelLocked = frcpReview.PanelLocked;

            // Check TempData for immediate lock changes (for same session)
            if (TempData[$"FRCP_{targetUserId}_ES_Locked"] != null)
            {
                esLocked = TempData[$"FRCP_{targetUserId}_ES_Locked"]?.ToString() == "true";
                TempData.Keep($"FRCP_{targetUserId}_ES_Locked");
            }

            if (TempData[$"FRCP_{targetUserId}_EYD_Locked"] != null)
            {
                eydLocked = TempData[$"FRCP_{targetUserId}_EYD_Locked"]?.ToString() == "true";
                TempData.Keep($"FRCP_{targetUserId}_EYD_Locked");
            }

            if (TempData[$"FRCP_{targetUserId}_Panel_Locked"] != null)
            {
                panelLocked = TempData[$"FRCP_{targetUserId}_Panel_Locked"]?.ToString() == "true";
                TempData.Keep($"FRCP_{targetUserId}_Panel_Locked");
            }

            // Get workflow status using the same logic as Portfolio page
            var frcpStatus = GetFRCPStatus(targetUserId);
            var esStatus = frcpStatus.ESStatus;
            var eydStatus = frcpStatus.EYDStatus;
            var panelStatus = frcpStatus.PanelStatus;

            // Determine edit permissions based on workflow status and locks
            bool canEditES = currentUser.Role == "ES" && !esLocked;
            
            bool canEditEYD = currentUser.Role == "EYD" && targetUserId == currentUser.Id && !eydLocked;
            
            // TPD/Dean can always access their Panel section regardless of ES/EYD completion
            bool canEditPanel = (currentUser.Role == "TPD" || currentUser.Role == "Dean") && !panelLocked;
            
            bool canUnlock = currentUser.Role == "Admin" || currentUser.Role == "TPD";

            // Load saved form data if it exists
            var savedESData = new Dictionary<string, string>();
            var savedEYDData = new Dictionary<string, string>();
            var savedPanelData = new Dictionary<string, string>();

            if (TempData[$"FRCP_{targetUserId}_ES"] != null)
            {
                var jsonData = TempData[$"FRCP_{targetUserId}_ES"]?.ToString();
                if (jsonData != null)
                {
                    savedESData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                }
                TempData.Keep($"FRCP_{targetUserId}_ES"); // Keep for next request
            }

            if (TempData[$"FRCP_{targetUserId}_EYD"] != null)
            {
                var jsonData = TempData[$"FRCP_{targetUserId}_EYD"]?.ToString();
                if (jsonData != null)
                {
                    savedEYDData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                }
                TempData.Keep($"FRCP_{targetUserId}_EYD"); // Keep for next request
            }

            if (TempData[$"FRCP_{targetUserId}_Panel"] != null)
            {
                var jsonData = TempData[$"FRCP_{targetUserId}_Panel"]?.ToString();
                if (jsonData != null)
                {
                    savedPanelData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                }
                TempData.Keep($"FRCP_{targetUserId}_Panel"); // Keep for next request
            }

            // Create view model
            var viewModel = new
            {
                UserId = targetUser.Id,
                UserName = targetUser.DisplayName ?? targetUser.UserName,
                CurrentUserRole = currentUser.Role,
                IsCurrentUser = targetUserId == currentUser.Id,
                
                // Edit permissions
                CanEditES = canEditES,
                CanEditEYD = canEditEYD,
                CanEditPanel = canEditPanel,
                CanUnlock = canUnlock,
                
                // Lock status
                ESLocked = esLocked,
                EYDLocked = eydLocked,
                PanelLocked = panelLocked,
                
                // Workflow status (mock for now)
                ESStatus = esStatus,
                EYDStatus = eydStatus,
                PanelStatus = panelStatus,
                
                // Real EPA matrix data
                EPAData = epaMatrixData,
                ActivityColumns = activityColumns,
                
                // Saved form data
                SavedESData = savedESData,
                SavedEYDData = savedEYDData,
                SavedPanelData = savedPanelData
            };

            return View("InterimReview", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveFRCPSection(string section, string userId, string action)
        {
            try
            {
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction("FinalReview", new { id = userId });
                }

                // For now, simulate saving by storing form data in TempData
                // This will persist the data across the redirect so user can see it was saved
                var formData = new Dictionary<string, string>();
                foreach (var key in Request.Form.Keys)
                {
                    formData[key] = Request.Form[key].ToString();
                }
                
                // Store the form data for this user and section
                var sessionKey = $"FRCP_{userId}_{section}";
                TempData[sessionKey] = System.Text.Json.JsonSerializer.Serialize(formData);

                bool isSubmit = action == "submit";

                // If submitting, lock the section
                if (isSubmit)
                {
                    var lockKey = $"FRCP_{userId}_{section}_Locked";
                    TempData[lockKey] = "true";
                    TempData.Keep(lockKey);
                }

                // Set success message
                TempData["SuccessMessage"] = isSubmit ? "Section submitted and locked successfully" : "Progress saved successfully";
                
                // Redirect back to the FinalReview page for the same user
                return RedirectToAction("FinalReview", new { id = userId });
            }
            catch (Exception ex)
            {
                // Set error message and redirect back
                TempData["ErrorMessage"] = $"Error saving section: {ex.Message}";
                return RedirectToAction("FinalReview", new { id = userId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockFRCPSection(string section, string userId)
        {
            try
            {
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction("FinalReview", new { id = userId });
                }

                // Check if user has permission to unlock (Admin, TPD, or Dean)
                if (currentUser.Role != "Admin" && currentUser.Role != "TPD" && currentUser.Role != "Dean")
                {
                    TempData["ErrorMessage"] = "You don't have permission to unlock sections";
                    return RedirectToAction("FinalReview", new { id = userId });
                }

                // Find or create the FRCP review record
                var frcpReview = await _context.FRCPReviews
                    .FirstOrDefaultAsync(r => r.EYDUserId == userId);

                if (frcpReview != null)
                {
                    // Unlock the specific section in the database
                    switch (section.ToUpper())
                    {
                        case "ES":
                            frcpReview.ESLocked = false;
                            break;
                        case "EYD":
                            frcpReview.EYDLocked = false;
                            break;
                        case "PANEL":
                            frcpReview.PanelLocked = false;
                            break;
                        default:
                            TempData["ErrorMessage"] = "Invalid section specified";
                            return RedirectToAction("FinalReview", new { id = userId });
                    }

                    frcpReview.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Also remove TempData lock for immediate effect
                var lockKey = $"FRCP_{userId}_{section}_Locked";
                TempData.Remove(lockKey);

                TempData["SuccessMessage"] = $"{section} section unlocked successfully";
                return RedirectToAction("FinalReview", new { id = userId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error unlocking section: {ex.Message}";
                return RedirectToAction("FinalReview", new { id = userId });
            }
        }
        
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

        private (string ESStatus, string EYDStatus, string PanelStatus) GetIRCPStatus(string userId)
        {
            var esStatus = "NotStarted";
            var eydStatus = "NotStarted";
            var panelStatus = "NotStarted";

            // Check for section locks first (both TempData and database)
            bool esLocked = TempData[$"IRCP_{userId}_ES_Locked"]?.ToString() == "true";
            bool eydLocked = TempData[$"IRCP_{userId}_EYD_Locked"]?.ToString() == "true";
            bool panelLocked = TempData[$"IRCP_{userId}_Panel_Locked"]?.ToString() == "true";

            // Check database for locks if not found in TempData
            var ircpReview = _context.IRCPReviews
                .FirstOrDefault(r => r.EYDUserId == userId);

            if (ircpReview != null)
            {
                // Update lock status from database if not set in TempData
                if (!esLocked) esLocked = ircpReview.ESLocked;
                if (!eydLocked) eydLocked = ircpReview.EYDLocked;
                if (!panelLocked) panelLocked = ircpReview.PanelLocked;
            }

            // Debug output
            Console.WriteLine($"DEBUG IRCP Status for {userId}:");
            Console.WriteLine($"  TempData ES_Locked: {TempData[$"IRCP_{userId}_ES_Locked"]?.ToString()}");
            Console.WriteLine($"  TempData EYD_Locked: {TempData[$"IRCP_{userId}_EYD_Locked"]?.ToString()}");
            Console.WriteLine($"  TempData Panel_Locked: {TempData[$"IRCP_{userId}_Panel_Locked"]?.ToString()}");
            if (ircpReview != null)
            {
                Console.WriteLine($"  DB ESLocked: {ircpReview.ESLocked}, ESStatus: {ircpReview.ESStatus}");
                Console.WriteLine($"  DB EYDLocked: {ircpReview.EYDLocked}, EYDStatus: {ircpReview.EYDStatus}");
                Console.WriteLine($"  DB PanelLocked: {ircpReview.PanelLocked}, PanelStatus: {ircpReview.PanelStatus}");
            }
            Console.WriteLine($"  Final locks: ES={esLocked}, EYD={eydLocked}, Panel={panelLocked}");

            // Check ES status
            if (esLocked)
            {
                esStatus = "Completed";
            }
            else if (TempData[$"IRCP_{userId}_ES"] != null)
            {
                var jsonData = TempData[$"IRCP_{userId}_ES"]?.ToString();
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var esData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                    if (esData.ContainsKey("ESConfirmation") && esData["ESConfirmation"] == "true")
                    {
                        esStatus = "Completed";
                    }
                    else if (esData.Count > 0)
                    {
                        esStatus = "InProgress";
                    }
                }
            }
            else if (ircpReview != null)
            {
                // Fallback to database status if no TempData
                esStatus = ircpReview.ESStatus.ToString();
            }

            // Check EYD status
            if (eydLocked)
            {
                eydStatus = "Completed";
            }
            else if (TempData[$"IRCP_{userId}_EYD"] != null)
            {
                var jsonData = TempData[$"IRCP_{userId}_EYD"]?.ToString();
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var eydData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                    if (eydData.Count > 0)
                    {
                        eydStatus = "InProgress";
                    }
                }
            }
            else if (ircpReview != null)
            {
                // Fallback to database status if no TempData
                eydStatus = ircpReview.EYDStatus.ToString();
            }

            // Check Panel status
            if (panelLocked)
            {
                panelStatus = "Completed";
            }
            else if (TempData[$"IRCP_{userId}_Panel"] != null)
            {
                var jsonData = TempData[$"IRCP_{userId}_Panel"]?.ToString();
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var panelData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                    if (panelData.Count > 0)
                    {
                        panelStatus = "InProgress";
                    }
                }
            }
            else if (ircpReview != null)
            {
                // Fallback to database status if no TempData
                panelStatus = ircpReview.PanelStatus.ToString();
            }

            Console.WriteLine($"  Final status: ES={esStatus}, EYD={eydStatus}, Panel={panelStatus}");

            // Keep TempData for future requests
            if (TempData[$"IRCP_{userId}_ES"] != null) TempData.Keep($"IRCP_{userId}_ES");
            if (TempData[$"IRCP_{userId}_EYD"] != null) TempData.Keep($"IRCP_{userId}_EYD");
            if (TempData[$"IRCP_{userId}_Panel"] != null) TempData.Keep($"IRCP_{userId}_Panel");
            if (esLocked) TempData.Keep($"IRCP_{userId}_ES_Locked");
            if (eydLocked) TempData.Keep($"IRCP_{userId}_EYD_Locked");
            if (panelLocked) TempData.Keep($"IRCP_{userId}_Panel_Locked");

            return (esStatus, eydStatus, panelStatus);
        }

        private (string ESStatus, string EYDStatus, string PanelStatus) GetFRCPStatus(string userId)
        {
            var esStatus = "NotStarted";
            var eydStatus = "NotStarted";
            var panelStatus = "NotStarted";

            // Check for section locks first
            bool esLocked = TempData[$"FRCP_{userId}_ES_Locked"]?.ToString() == "true";
            bool eydLocked = TempData[$"FRCP_{userId}_EYD_Locked"]?.ToString() == "true";
            bool panelLocked = TempData[$"FRCP_{userId}_Panel_Locked"]?.ToString() == "true";

            // Check ES status
            if (esLocked)
            {
                esStatus = "Completed";
            }
            else if (TempData[$"FRCP_{userId}_ES"] != null)
            {
                var jsonData = TempData[$"FRCP_{userId}_ES"]?.ToString();
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var esData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                    if (esData.ContainsKey("ESConfirmation") && esData["ESConfirmation"] == "true")
                    {
                        esStatus = "Completed";
                    }
                    else if (esData.Count > 0)
                    {
                        esStatus = "InProgress";
                    }
                }
            }

            // Check EYD status
            if (eydLocked)
            {
                eydStatus = "Completed";
            }
            else if (TempData[$"FRCP_{userId}_EYD"] != null)
            {
                var jsonData = TempData[$"FRCP_{userId}_EYD"]?.ToString();
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var eydData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                    if (eydData.Count > 0)
                    {
                        eydStatus = "InProgress";
                    }
                }
            }

            // Check Panel status
            if (panelLocked)
            {
                panelStatus = "Completed";
            }
            else if (TempData[$"FRCP_{userId}_Panel"] != null)
            {
                var jsonData = TempData[$"FRCP_{userId}_Panel"]?.ToString();
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var panelData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
                    if (panelData.Count > 0)
                    {
                        panelStatus = "InProgress";
                    }
                }
            }

            // Keep TempData for future requests
            if (TempData[$"FRCP_{userId}_ES"] != null) TempData.Keep($"FRCP_{userId}_ES");
            if (TempData[$"FRCP_{userId}_EYD"] != null) TempData.Keep($"FRCP_{userId}_EYD");
            if (TempData[$"FRCP_{userId}_Panel"] != null) TempData.Keep($"FRCP_{userId}_Panel");
            if (esLocked) TempData.Keep($"FRCP_{userId}_ES_Locked");
            if (eydLocked) TempData.Keep($"FRCP_{userId}_EYD_Locked");
            if (panelLocked) TempData.Keep($"FRCP_{userId}_Panel_Locked");

            return (esStatus, eydStatus, panelStatus);
        }
        
        private async Task<string> GetPSQStatusAsync(string userId)
        {
            try
            {
                // Get the PSQ questionnaire for this user
                var questionnaire = await _context.PSQQuestionnaires
                    .FirstOrDefaultAsync(q => q.PerformerId == userId);

                if (questionnaire == null)
                {
                    return "NotStarted"; // Red - No questionnaire created yet
                }

                // Count the responses for this questionnaire
                var responseCount = await _context.PSQResponses
                    .CountAsync(r => r.PSQQuestionnaireId == questionnaire.Id);

                // Apply traffic light logic
                if (responseCount >= 20)
                {
                    return "Completed"; // Green - 20+ responses
                }
                else if (responseCount >= 1)
                {
                    return "InProgress"; // Amber - 1-19 responses
                }
                else
                {
                    return "NotStarted"; // Red - 0 responses
                }
            }
            catch (Exception)
            {
                // On error, default to NotStarted
                return "NotStarted";
            }
        }
        
        private async Task<string> GetMSFStatusAsync(string userId)
        {
            try
            {
                // Get the MSF questionnaire for this user
                var questionnaire = await _context.MSFQuestionnaires
                    .FirstOrDefaultAsync(q => q.PerformerId == userId);

                if (questionnaire == null)
                {
                    return "NotStarted"; // Red - No questionnaire created yet
                }

                // Count the responses for this questionnaire
                var responseCount = await _context.MSFResponses
                    .CountAsync(r => r.MSFQuestionnaireId == questionnaire.Id);

                // Apply traffic light logic
                if (responseCount >= 8)
                {
                    return "Completed"; // Green - 8+ responses
                }
                else if (responseCount >= 1)
                {
                    return "InProgress"; // Amber - 1-7 responses
                }
                else
                {
                    return "NotStarted"; // Red - 0 responses
                }
            }
            catch (Exception)
            {
                // On error, default to NotStarted
                return "NotStarted";
            }
        }
        
        private async Task<string> GetAdHocESReportStatusAsync(string userId)
        {
            try
            {
                // Get the Ad Hoc ES Report for this user
                var adHocReport = await _context.AdHocESReports
                    .FirstOrDefaultAsync(r => r.EYDUserId == userId);

                if (adHocReport == null)
                {
                    return "NotStarted"; // Red - No report exists
                }

                // Check if the report is completed
                if (adHocReport.IsESCompleted && adHocReport.IsEYDCompleted)
                {
                    return "Completed"; // Green - Both ES and EYD sections completed
                }
                else if (!string.IsNullOrEmpty(adHocReport.ESOverallAssessment) || 
                         !string.IsNullOrEmpty(adHocReport.EYDReflectionComments))
                {
                    return "InProgress"; // Amber - Some data saved but not completed
                }
                else
                {
                    return "NotStarted"; // Red - Report exists but no content
                }
            }
            catch (Exception)
            {
                // On error, default to NotStarted
                return "NotStarted";
            }
        }
        
        private async Task<(string ESStatus, string EYDStatus)> GetAdHocESReportStatusesAsync(string userId)
        {
            try
            {
                // Get the Ad Hoc ES Report for this user
                var adHocReport = await _context.AdHocESReports
                    .FirstOrDefaultAsync(r => r.EYDUserId == userId);

                if (adHocReport == null)
                {
                    return ("NotStarted", "NotStarted"); // Red - No report exists
                }

                // Check ES Section Status
                string esStatus;
                if (adHocReport.IsESCompleted)
                {
                    esStatus = "Completed"; // Green - ES section completed
                }
                else if (!string.IsNullOrEmpty(adHocReport.ESOverallAssessment) || 
                         !string.IsNullOrEmpty(adHocReport.ESStrengths) ||
                         !string.IsNullOrEmpty(adHocReport.ESAreasForDevelopment))
                {
                    esStatus = "InProgress"; // Amber - Some ES data saved
                }
                else
                {
                    esStatus = "NotStarted"; // Red - No ES content
                }

                // Check EYD Section Status
                string eydStatus;
                if (adHocReport.IsEYDCompleted)
                {
                    eydStatus = "Completed"; // Green - EYD section completed
                }
                else if (!string.IsNullOrEmpty(adHocReport.EYDReflectionComments) ||
                         !string.IsNullOrEmpty(adHocReport.EYDLearningGoals))
                {
                    eydStatus = "InProgress"; // Amber - Some EYD data saved
                }
                else
                {
                    eydStatus = "NotStarted"; // Red - No EYD content
                }

                return (esStatus, eydStatus);
            }
            catch (Exception)
            {
                // On error, default to NotStarted
                return ("NotStarted", "NotStarted");
            }
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

        private string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<PSQResultsDto> GetPSQResults(int questionnaireId, ApplicationUser targetUser)
        {
            var questionnaire = await _context.PSQQuestionnaires
                .FirstOrDefaultAsync(q => q.Id == questionnaireId);

            if (questionnaire == null)
            {
                throw new InvalidOperationException("Questionnaire not found");
            }

            var responses = await _context.PSQResponses
                .Where(r => r.PSQQuestionnaireId == questionnaireId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            var feedbackUrl = Url.Action("PSQFeedback", "PSQ", 
                new { code = questionnaire.UniqueCode }, Request.Scheme) ?? "";

            var results = new PSQResultsDto
            {
                Questionnaire = questionnaire,
                TotalResponses = responses.Count,
                PerformerName = targetUser.DisplayName ?? targetUser.UserName ?? "Unknown User",
                FeedbackUrl = feedbackUrl,
                QuestionAverages = CalculateQuestionAverages(responses),
                PositiveComments = responses
                    .Where(r => !string.IsNullOrEmpty(r.DoesWellComment))
                    .Select(r => r.DoesWellComment!)
                    .ToList(),
                ImprovementComments = responses
                    .Where(r => !string.IsNullOrEmpty(r.CouldImproveComment))
                    .Select(r => r.CouldImproveComment!)
                    .ToList(),
                RecentResponses = responses.Take(10).ToList()
            };

            if (results.QuestionAverages.Any())
            {
                results.OverallAverage = results.QuestionAverages.Values.Average();
            }

            return results;
        }

        private Dictionary<string, double> CalculateQuestionAverages(List<PSQResponse> responses)
        {
            var questionAverages = new Dictionary<string, double>();
            
            if (!responses.Any()) return questionAverages;

            var questions = new[]
            {
                ("PutMeAtEase", "The Dentist put me at ease"),
                ("TreatedWithDignity", "Treated me with dignity and respect"),
                ("ListenedToConcerns", "Listened and responded to my concerns"),
                ("ExplainedTreatmentOptions", "Clearly explained treatment options including costs"),
                ("InvolvedInDecisions", "Involved me in decisions about my care"),
                ("InvolvedFamily", "Involved family/carers appropriately"),
                ("TailoredApproach", "Tailored approach to meet my needs"),
                ("ExplainedNextSteps", "Explained what will happen next with treatment"),
                ("ProvidedGuidance", "Provided guidance on dental care"),
                ("AllocatedTime", "Allocated right amount of time for treatment"),
                ("WorkedWithTeam", "Worked well with other team members"),
                ("CanTrustDentist", "Can trust this dentist with dental care")
            };

            foreach (var (key, title) in questions)
            {
                var scores = GetScoresForQuestion(responses, key)
                    .Where(score => score != 999) // Exclude "Not observed"
                    .ToList();

                if (scores.Any())
                {
                    questionAverages[key] = scores.Average(); // Use the key instead of title
                }
                else
                {
                    questionAverages[key] = 0.0; // Default to 0 if no valid scores
                }
            }

            return questionAverages;
        }

        private List<int> GetScoresForQuestion(List<PSQResponse> responses, string questionKey)
        {
            return questionKey switch
            {
                "PutMeAtEase" => responses.Where(r => r.PutMeAtEaseScore.HasValue).Select(r => r.PutMeAtEaseScore!.Value).ToList(),
                "TreatedWithDignity" => responses.Where(r => r.TreatedWithDignityScore.HasValue).Select(r => r.TreatedWithDignityScore!.Value).ToList(),
                "ListenedToConcerns" => responses.Where(r => r.ListenedToConcernsScore.HasValue).Select(r => r.ListenedToConcernsScore!.Value).ToList(),
                "ExplainedTreatmentOptions" => responses.Where(r => r.ExplainedTreatmentOptionsScore.HasValue).Select(r => r.ExplainedTreatmentOptionsScore!.Value).ToList(),
                "InvolvedInDecisions" => responses.Where(r => r.InvolvedInDecisionsScore.HasValue).Select(r => r.InvolvedInDecisionsScore!.Value).ToList(),
                "InvolvedFamily" => responses.Where(r => r.InvolvedFamilyScore.HasValue).Select(r => r.InvolvedFamilyScore!.Value).ToList(),
                "TailoredApproach" => responses.Where(r => r.TailoredApproachScore.HasValue).Select(r => r.TailoredApproachScore!.Value).ToList(),
                "ExplainedNextSteps" => responses.Where(r => r.ExplainedNextStepsScore.HasValue).Select(r => r.ExplainedNextStepsScore!.Value).ToList(),
                "ProvidedGuidance" => responses.Where(r => r.ProvidedGuidanceScore.HasValue).Select(r => r.ProvidedGuidanceScore!.Value).ToList(),
                "AllocatedTime" => responses.Where(r => r.AllocatedTimeScore.HasValue).Select(r => r.AllocatedTimeScore!.Value).ToList(),
                "WorkedWithTeam" => responses.Where(r => r.WorkedWithTeamScore.HasValue).Select(r => r.WorkedWithTeamScore!.Value).ToList(),
                "CanTrustDentist" => responses.Where(r => r.CanTrustDentistScore.HasValue).Select(r => r.CanTrustDentistScore!.Value).ToList(),
                _ => new List<int>()
            };
        }

        private async Task<MSFResultsDto> GetMSFResults(int questionnaireId, ApplicationUser targetUser)
        {
            var questionnaire = await _context.MSFQuestionnaires
                .FirstOrDefaultAsync(q => q.Id == questionnaireId);

            if (questionnaire == null)
            {
                throw new InvalidOperationException("MSF Questionnaire not found");
            }

            var responses = await _context.MSFResponses
                .Where(r => r.MSFQuestionnaireId == questionnaireId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            var feedbackUrl = Url.Action("MSFFeedback", "MSF", 
                new { code = questionnaire.UniqueCode }, Request.Scheme) ?? "";

            var results = new MSFResultsDto
            {
                Questionnaire = questionnaire,
                TotalResponses = responses.Count,
                PerformerName = targetUser.DisplayName ?? targetUser.UserName ?? "Unknown User",
                FeedbackUrl = feedbackUrl,
                QuestionAverages = CalculateMSFQuestionAverages(responses),
                PositiveComments = responses
                    .Where(r => !string.IsNullOrEmpty(r.DoesWellComment))
                    .Select(r => r.DoesWellComment!)
                    .ToList(),
                ImprovementComments = responses
                    .Where(r => !string.IsNullOrEmpty(r.CouldImproveComment))
                    .Select(r => r.CouldImproveComment!)
                    .ToList(),
                RecentResponses = responses.Take(10).ToList()
            };

            if (results.QuestionAverages.Any())
            {
                results.OverallAverage = results.QuestionAverages.Values.Average();
                
                // Calculate topic averages
                var communicationQuestions = new[] { "TreatWithCompassion", "EnableInformedDecisions", "RecogniseCommunicationNeeds", "ProduceClearCommunications" };
                var professionalismQuestions = new[] { "DemonstrateIntegrity", "WorkWithinScope", "EngageWithDevelopment", "KeepPracticeUpToDate", "FacilitateLearning", "InteractWithColleagues", "PromoteEquality" };
                var managementQuestions = new[] { "RecogniseImpactOfBehaviours", "ManageTimeAndResources", "WorkAsTeamMember", "WorkToStandards", "ParticipateInImprovement", "MinimiseWaste" };
                
                var communicationScores = results.QuestionAverages.Where(q => communicationQuestions.Contains(q.Key)).Select(q => q.Value);
                var professionalismScores = results.QuestionAverages.Where(q => professionalismQuestions.Contains(q.Key)).Select(q => q.Value);
                var managementScores = results.QuestionAverages.Where(q => managementQuestions.Contains(q.Key)).Select(q => q.Value);
                
                results.CommunicationAverage = communicationScores.Any() ? communicationScores.Average() : 0.0;
                results.ProfessionalismAverage = professionalismScores.Any() ? professionalismScores.Average() : 0.0;
                results.ManagementLeadershipAverage = managementScores.Any() ? managementScores.Average() : 0.0;
            }

            return results;
        }

        private Dictionary<string, double> CalculateMSFQuestionAverages(List<MSFResponse> responses)
        {
            var questionAverages = new Dictionary<string, double>();
            
            if (!responses.Any()) return questionAverages;

            var questions = new[]
            {
                // Communication Topic
                ("TreatWithCompassion", "Treat patients, carers and colleagues with compassion, dignity and respect"),
                ("EnableInformedDecisions", "Enable patients to make informed decisions about their care"),
                ("RecogniseCommunicationNeeds", "Recognise and respond appropriately to the individual communication needs of all patients"),
                ("ProduceClearCommunications", "Produce clearly written, timely and appropriate professional communications"),
                
                // Professionalism Topic
                ("DemonstrateIntegrity", "Demonstrate integrity and honesty in all professional interactions"),
                ("WorkWithinScope", "Work within my permitted scope of practice, seeking guidance or support when needed"),
                ("EngageWithDevelopment", "Engage with opportunities to develop my professional practice"),
                ("KeepPracticeUpToDate", "Routinely takes steps to keep my practice up to date"),
                ("FacilitateLearning", "Facilitate the learning of students and/or colleagues"),
                ("InteractWithColleagues", "Interact with colleagues in ways that recognise and value the contributions of the wider dental team"),
                ("PromoteEquality", "Actively promote equality, diversity and inclusion in all aspects of my work"),
                
                // Management and Leadership Topic
                ("RecogniseImpactOfBehaviours", "Work in ways that recognise the impact of my behaviours on others"),
                ("ManageTimeAndResources", "Manage time and resources effectively and efficiently"),
                ("WorkAsTeamMember", "Work well as a team member, taking the lead as appropriate"),
                ("WorkToStandards", "Routinely work in ways consistent with relevant professional standards and legislation"),
                ("ParticipateInImprovement", "Participate in projects and activities designed to improve the quality of care"),
                ("MinimiseWaste", "Work in ways that minimise waste and reduce harmful environment impact")
            };

            foreach (var (key, title) in questions)
            {
                var scores = GetMSFScoresForQuestion(responses, key)
                    .Where(score => score != 0) // Exclude "Not Applicable" (0) responses
                    .ToList();

                if (scores.Any())
                {
                    questionAverages[key] = scores.Average();
                }
                else
                {
                    questionAverages[key] = 0.0;
                }
            }

            return questionAverages;
        }

        private List<int> GetMSFScoresForQuestion(List<MSFResponse> responses, string questionKey)
        {
            return questionKey switch
            {
                // Communication Topic
                "TreatWithCompassion" => responses.Where(r => r.TreatWithCompassionScore.HasValue).Select(r => r.TreatWithCompassionScore!.Value).ToList(),
                "EnableInformedDecisions" => responses.Where(r => r.EnableInformedDecisionsScore.HasValue).Select(r => r.EnableInformedDecisionsScore!.Value).ToList(),
                "RecogniseCommunicationNeeds" => responses.Where(r => r.RecogniseCommunicationNeedsScore.HasValue).Select(r => r.RecogniseCommunicationNeedsScore!.Value).ToList(),
                "ProduceClearCommunications" => responses.Where(r => r.ProduceClearCommunicationsScore.HasValue).Select(r => r.ProduceClearCommunicationsScore!.Value).ToList(),
                
                // Professionalism Topic
                "DemonstrateIntegrity" => responses.Where(r => r.DemonstrateIntegrityScore.HasValue).Select(r => r.DemonstrateIntegrityScore!.Value).ToList(),
                "WorkWithinScope" => responses.Where(r => r.WorkWithinScopeScore.HasValue).Select(r => r.WorkWithinScopeScore!.Value).ToList(),
                "EngageWithDevelopment" => responses.Where(r => r.EngageWithDevelopmentScore.HasValue).Select(r => r.EngageWithDevelopmentScore!.Value).ToList(),
                "KeepPracticeUpToDate" => responses.Where(r => r.KeepPracticeUpToDateScore.HasValue).Select(r => r.KeepPracticeUpToDateScore!.Value).ToList(),
                "FacilitateLearning" => responses.Where(r => r.FacilitateLearningScore.HasValue).Select(r => r.FacilitateLearningScore!.Value).ToList(),
                "InteractWithColleagues" => responses.Where(r => r.InteractWithColleaguesScore.HasValue).Select(r => r.InteractWithColleaguesScore!.Value).ToList(),
                "PromoteEquality" => responses.Where(r => r.PromoteEqualityScore.HasValue).Select(r => r.PromoteEqualityScore!.Value).ToList(),
                
                // Management and Leadership Topic
                "RecogniseImpactOfBehaviours" => responses.Where(r => r.RecogniseImpactOfBehavioursScore.HasValue).Select(r => r.RecogniseImpactOfBehavioursScore!.Value).ToList(),
                "ManageTimeAndResources" => responses.Where(r => r.ManageTimeAndResourcesScore.HasValue).Select(r => r.ManageTimeAndResourcesScore!.Value).ToList(),
                "WorkAsTeamMember" => responses.Where(r => r.WorkAsTeamMemberScore.HasValue).Select(r => r.WorkAsTeamMemberScore!.Value).ToList(),
                "WorkToStandards" => responses.Where(r => r.WorkToStandardsScore.HasValue).Select(r => r.WorkToStandardsScore!.Value).ToList(),
                "ParticipateInImprovement" => responses.Where(r => r.ParticipateInImprovementScore.HasValue).Select(r => r.ParticipateInImprovementScore!.Value).ToList(),
                "MinimiseWaste" => responses.Where(r => r.MinimiseWasteScore.HasValue).Select(r => r.MinimiseWasteScore!.Value).ToList(),
                _ => new List<int>()
            };
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveAdHocESSection(string section, string userId, string action)
        {
            try
            {
                // Get or create Ad Hoc ES Report
                var report = await _context.AdHocESReports
                    .Include(r => r.EPAAssessments)
                    .FirstOrDefaultAsync(r => r.EYDUserId == userId);

                if (report == null)
                {
                    report = new AdHocESReport
                    {
                        EYDUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AdHocESReports.Add(report);
                    await _context.SaveChangesAsync();
                }

                if (section == "ES")
                {
                    // Update ES section fields (mapped to our 5 required fields)
                    report.ESOverallAssessment = Request.Form["ESOverallAssessment"]; // Performance to Date
                    report.ESProgressSinceLastReview = Request.Form["ESProgressSinceLastReview"]; // Progress made since start of training
                    report.ESStrengths = Request.Form["ESStrengths"]; // Areas of notable practice
                    report.ESAreasForDevelopment = Request.Form["ESAreasForDevelopment"]; // Any performance concerns
                    report.ESRecommendations = Request.Form["ESRecommendations"]; // Immediate priorities for development
                    
                    // Handle confirmation checkbox
                    var confirmationValue = Request.Form["ESConfirmation"].ToString();
                    // Note: Checkbox confirmation is captured but stored in existing fields above

                    // Handle EPA assessments
                    var epas = await _epaService.GetAllActiveEPAsAsync();
                    foreach (var epa in epas)
                    {
                        var progressLevel = Request.Form[$"EPA_{epa.Id}_ProgressLevel"].ToString();
                        var comments = Request.Form[$"EPA_{epa.Id}_Comments"].ToString();

                        if (!string.IsNullOrEmpty(progressLevel) || !string.IsNullOrEmpty(comments))
                        {
                            var assessment = report.EPAAssessments.FirstOrDefault(a => a.EPAId == epa.Id);
                            if (assessment == null)
                            {
                                assessment = new AdHocESReportEPAAssessment
                                {
                                    AdHocESReportId = report.Id,
                                    EPAId = epa.Id,
                                    CreatedAt = DateTime.UtcNow
                                };
                                report.EPAAssessments.Add(assessment);
                            }
                            assessment.ProgressLevel = progressLevel;
                            assessment.Comments = comments;
                            assessment.UpdatedAt = DateTime.UtcNow;
                        }
                    }

                    if (action == "complete")
                    {
                        report.IsESCompleted = true;
                        report.ESCompletedAt = DateTime.UtcNow;
                        TempData["SuccessMessage"] = "ES assessment completed and locked successfully.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "ES assessment saved successfully.";
                    }
                }
                else if (section == "EYD")
                {
                    // Update EYD section fields
                    report.EYDReflectionComments = Request.Form["EYDReflectionComments"];

                    if (action == "complete")
                    {
                        report.IsEYDCompleted = true;
                        report.EYDCompletedAt = DateTime.UtcNow;
                        TempData["SuccessMessage"] = "EYD reflection completed and submitted successfully.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "EYD reflection saved successfully.";
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("AdHocESReport", new { id = userId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error saving data: {ex.Message}";
                return RedirectToAction("AdHocESReport", new { id = userId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockAdHocESSection(string section, string userId)
        {
            try
            {
                var report = await _context.AdHocESReports
                    .FirstOrDefaultAsync(r => r.EYDUserId == userId);

                if (report != null)
                {
                    if (section == "ES")
                    {
                        report.IsESCompleted = false;
                        report.ESCompletedAt = null;
                        TempData["SuccessMessage"] = "ES section unlocked successfully.";
                    }
                    else if (section == "EYD")
                    {
                        report.IsEYDCompleted = false;
                        report.EYDCompletedAt = null;
                        TempData["SuccessMessage"] = "EYD section unlocked successfully.";
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("AdHocESReport", new { id = userId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error unlocking section: {ex.Message}";
                return RedirectToAction("AdHocESReport", new { id = userId });
            }
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
        
        // IRCP Status indicators
        public string IRCPESStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public string IRCPEYDStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public string IRCPPanelStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        
        // FRCP Status indicators
        public string FRCPESStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public string FRCPEYDStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public string FRCPPanelStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        
        // PSQ Traffic Light Status
        public string PSQStatus { get; set; } = "NotStarted"; // NotStarted (red, 0 responses), InProgress (amber, 1+ responses), Completed (green, 20+ responses)
        
        // MSF Traffic Light Status
        public string MSFStatus { get; set; } = "NotStarted"; // NotStarted (red, 0 responses), InProgress (amber, 1-7 responses), Completed (green, 8+ responses)
        
        // Ad Hoc ES Report Traffic Light Status
        public string AdHocESReportESStatus { get; set; } = "NotStarted"; // NotStarted (red, not started), InProgress (yellow, draft saved), Completed (green, completed)
        public string AdHocESReportEYDStatus { get; set; } = "NotStarted"; // NotStarted (red, not started), InProgress (yellow, draft saved), Completed (green, completed)
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
