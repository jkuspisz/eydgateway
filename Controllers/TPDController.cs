using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;
using EYDGateway.Services;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class TPDController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIAnalysisService _aiAnalysis;

        public TPDController(ApplicationDbContext context, IAIAnalysisService aiAnalysis)
        {
            _context = context;
            _aiAnalysis = aiAnalysis;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Redirect to user-specific dashboard
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.Role != "TPD" && currentUser?.Role != "Dean")
            {
                return Unauthorized();
            }

            return RedirectToAction("UserDashboard", new { userId = currentUser.Id });
        }

        public async Task<IActionResult> UserDashboard(string? userId = null)
        {
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.Role != "TPD" && currentUser?.Role != "Dean")
            {
                return Unauthorized();
            }

            // If no userId provided, use current user's ID
            if (string.IsNullOrEmpty(userId))
            {
                userId = currentUser.Id;
            }

            // Security check: users can only access their own dashboard
            if (userId != currentUser.Id)
            {
                return Forbid("You can only access your own dashboard.");
            }

            // TPDs are assigned to a specific scheme, get the area through the scheme
            var assignedArea = currentUser.Scheme?.Area;
            var managedScheme = currentUser.Scheme;

            // Get all schemes in the TPD's area (for view-only dropdown)
            var allAreaSchemes = assignedArea != null 
                ? await _context.Schemes
                    .Where(s => s.AreaId == assignedArea.Id)
                    .Include(s => s.Area)
                    .ToListAsync()
                : new List<Scheme>();

            // Get all EYD users in the same scheme as this TPD (default view)
            var assignedUsers = await _context.Users
                .Where(u => u.SchemeId == currentUser.SchemeId && u.Role == "EYD")
                .Include(u => u.Scheme)
                    .ThenInclude(s => s!.Area)
                .ToListAsync();

            // Get pending assessment invitations for this TPD
            var pendingInvitations = await _context.SLEs
                .Where(sle => sle.AssessorUserId == currentUser.Id && 
                             sle.Status == "Invited" && 
                             !sle.IsAssessmentCompleted)
                .Include(sle => sle.EYDUser)
                .Include(sle => sle.EPAMappings)
                    .ThenInclude(em => em.EPA)
                .ToListAsync();

            // Generate portfolio summaries for assigned users
            var portfolioSummaries = await GeneratePortfolioSummaries(assignedUsers);

            var viewModel = new TPDDashboardViewModel
            {
                UserId = currentUser.Id,
                UserName = currentUser.DisplayName ?? currentUser.UserName ?? "Unknown User",
                AssignedArea = assignedArea?.Name ?? "No Area Assigned",
                AssignedScheme = managedScheme?.Name ?? "No Scheme Assigned",
                AllAreaSchemes = allAreaSchemes,
                EYDUsers = assignedUsers,
                AssignedEYDUsers = assignedUsers, // Compatibility
                PendingInvitations = pendingInvitations, // Individual assessment tasks for this TPD
                CurrentSchemeId = currentUser.SchemeId, // Track the currently selected scheme
                EYDPortfolioSummaries = portfolioSummaries // Enhanced portfolio data
            };

            ViewBag.UserRole = currentUser.Role;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignEYDToScheme(string eydUserId, int schemeId)
        {
            // Assignment functionality has been moved to Admin-only access
            // TPD users can view assigned EYD users but cannot modify assignments
            TempData["ErrorMessage"] = "EYD assignment functionality is only available to administrators.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> ViewSchemeEYDs(int schemeId)
        {
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser?.Role != "TPD" && currentUser?.Role != "Dean")
            {
                return Unauthorized();
            }

            // Verify the scheme is in the TPD's area
            var scheme = await _context.Schemes
                .Include(s => s.Area)
                .FirstOrDefaultAsync(s => s.Id == schemeId);

            if (scheme?.AreaId != currentUser.Scheme?.AreaId)
            {
                return Unauthorized(); // Can only view schemes in their own area
            }

            // Get EYD users in the selected scheme
            var schemeEYDs = await _context.Users
                .Where(u => u.SchemeId == schemeId && u.Role == "EYD")
                .Include(u => u.Scheme)
                    .ThenInclude(s => s.Area)
                .ToListAsync();

            // Get all schemes in the area for the dropdown
            var allAreaSchemes = await _context.Schemes
                .Where(s => s.AreaId == currentUser.Scheme.Area.Id)
                .Include(s => s.Area)
                .ToListAsync();

            // Get pending assessment invitations for this TPD (same as dashboard)
            var pendingInvitations = await _context.SLEs
                .Where(sle => sle.AssessorUserId == currentUser.Id && 
                             sle.Status == "Invited" && 
                             !sle.IsAssessmentCompleted)
                .Include(sle => sle.EYDUser)
                .Include(sle => sle.EPAMappings)
                    .ThenInclude(em => em.EPA)
                .ToListAsync();

            // Generate portfolio summaries for the selected scheme's EYDs
            var portfolioSummaries = await GeneratePortfolioSummaries(schemeEYDs);

            var viewModel = new TPDDashboardViewModel
            {
                UserId = currentUser.Id,
                UserName = currentUser.DisplayName ?? currentUser.UserName ?? "Unknown User",
                AssignedArea = currentUser.Scheme?.Area?.Name ?? "No Area Assigned",
                AssignedScheme = currentUser.Scheme?.Name ?? "No Scheme Assigned",
                AllAreaSchemes = allAreaSchemes,
                EYDUsers = schemeEYDs,
                AssignedEYDUsers = schemeEYDs, // Compatibility
                PendingInvitations = pendingInvitations, // Individual assessment tasks for this TPD
                CurrentSchemeId = schemeId, // Track the currently selected scheme
                EYDPortfolioSummaries = portfolioSummaries // Enhanced portfolio data
            };

            ViewBag.UserRole = currentUser.Role;
            ViewBag.SelectedSchemeName = scheme.Name;
            ViewBag.IsViewingOtherScheme = schemeId != currentUser.SchemeId;

            return View("UserDashboard", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SearchEYD(string searchTerm)
        {
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser?.Role != "TPD" && currentUser?.Role != "Dean")
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["ErrorMessage"] = "Please enter a valid search term (username/GDC number).";
                return RedirectToAction("Dashboard");
            }

            // Search for EYD user by exact username match (case-insensitive)
            var foundEYD = await _context.Users
                .Where(u => u.Role == "EYD" && u.UserName.ToLower() == searchTerm.ToLower())
                .Include(u => u.Scheme)
                    .ThenInclude(s => s.Area)
                .FirstOrDefaultAsync();

            if (foundEYD == null)
            {
                TempData["ErrorMessage"] = $"No EYD user found with username '{searchTerm}'.";
                return RedirectToAction("Dashboard");
            }

            // Create a temporary result to show the found EYD
            TempData["SearchResult"] = $"Found EYD: {foundEYD.DisplayName} ({foundEYD.UserName}) - {foundEYD.Scheme?.Name} in {foundEYD.Scheme?.Area?.Name}";
            TempData["FoundEYDId"] = foundEYD.Id;
            TempData["FoundEYDName"] = foundEYD.DisplayName;
            TempData["FoundEYDUsername"] = foundEYD.UserName;
            TempData["FoundEYDScheme"] = foundEYD.Scheme?.Name;
            TempData["FoundEYDArea"] = foundEYD.Scheme?.Area?.Name;

            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> ViewSchemeProgress(int schemeId)
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var scheme = await _context.Schemes
                .Include(s => s.Area)
                .FirstOrDefaultAsync(s => s.Id == schemeId);

            if (scheme == null || currentUser?.SchemeId != schemeId)
            {
                return NotFound();
            }

            // Get all EYD users assigned to this scheme
            var assignedUsers = await _context.Users
                .Where(u => u.SchemeId == schemeId && u.Role == "EYD")
                .ToListAsync();

            var viewModel = new SchemeProgressViewModel
            {
                Scheme = scheme,
                AssignedEYDUsers = assignedUsers,
                TPDName = currentUser.DisplayName ?? currentUser.UserName
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AnalyzePortfolio(string eydUserId, string mode = "overall")
        {
            var currentUser = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.Role != "TPD" && currentUser?.Role != "Dean")
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(eydUserId))
            {
                return BadRequest(new { error = "Missing eydUserId" });
            }

            // Verify access: TPDs can only analyze EYDs within their area; Deans can analyze any
            var target = await _context.Users
                .Include(u => u.Scheme)
                    .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.Id == eydUserId && u.Role == "EYD");

            if (target == null)
            {
                return NotFound(new { error = "EYD not found" });
            }

            if (currentUser.Role == "TPD")
            {
                var currentAreaId = currentUser.Scheme?.AreaId;
                var targetAreaId = target.Scheme?.AreaId;
                if (!currentAreaId.HasValue || !targetAreaId.HasValue || currentAreaId.Value != targetAreaId.Value)
                {
                    return Forbid("Not permitted to analyze EYDs outside your area.");
                }
            }

            var result = await _aiAnalysis.AnalyzePortfolioAsync(eydUserId, mode);
            return Json(result);
        }

        private async Task<List<EYDPortfolioSummary>> GeneratePortfolioSummaries(List<ApplicationUser> eydUsers)
        {
            var summaries = new List<EYDPortfolioSummary>();

            foreach (var user in eydUsers)
            {
                var summary = new EYDPortfolioSummary
                {
                    UserId = user.Id,
                    UserName = user.DisplayName ?? user.UserName ?? "Unknown",
                    UserEmail = user.Email ?? "",
                    SchemeName = user.Scheme?.Name ?? "No Scheme"
                };

                // Individual SLE type data - using exact same logic as EYD Portfolio
                var cbdTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "CBD");
                var cbdComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "CBD" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                summary.CBDTotal = cbdTotal;
                summary.CBDCompleted = cbdComplete;

                var dopsTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DOPS");
                var dopsComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DOPS" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                summary.DOPSTotal = dopsTotal;
                summary.DOPSCompleted = dopsComplete;

                var miniCexTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "MiniCEX");
                var miniCexComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "MiniCEX" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                summary.MiniCEXTotal = miniCexTotal;
                summary.MiniCEXCompleted = miniCexComplete;

                var dopsSimTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DOPSSim");
                var dopsSimComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DOPSSim" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                summary.DOPSSimTotal = dopsSimTotal;
                summary.DOPSSimCompleted = dopsSimComplete;

                var dtctTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DtCT");
                var dtctComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DtCT" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                summary.DtCTTotal = dtctTotal;
                summary.DtCTCompleted = dtctComplete;

                var dentlTotal = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DENTL");
                var dentlComplete = await _context.SLEs.CountAsync(s => s.EYDUserId == user.Id && s.SLEType == "DENTL" && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                summary.DENTLTotal = dentlTotal;
                summary.DENTLCompleted = dentlComplete;

                // PLT data - using same logic as EYD Portfolio (IsLocked means completed)
                var pltTotal = await _context.ProtectedLearningTimes.CountAsync(plt => plt.UserId == user.Id);
                var pltComplete = await _context.ProtectedLearningTimes.CountAsync(plt => plt.UserId == user.Id && plt.IsLocked);
                summary.PLTTotal = pltTotal;
                summary.PLTCompleted = pltComplete;

                // Reflection data - using same logic as EYD Portfolio (IsLocked means completed)
                var reflections = await _context.Reflections
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();
                summary.ReflectionTotal = reflections.Count;
                summary.ReflectionCompleted = reflections.Count(r => r.IsLocked);

                // Learning Need data - using same completion logic
                var learningNeeds = await _context.LearningNeeds
                    .Where(l => l.UserId == user.Id)
                    .ToListAsync();
                summary.LearningNeedTotal = learningNeeds.Count;
                summary.LearningNeedCompleted = learningNeeds.Count(l => l.Status == LearningNeedStatus.Completed);

                // IRCP/FRCP Status - using exact same methods as EYD Portfolio
                var ircpStatus = GetIRCPStatus(user.Id);
                summary.IRCPESStatus = ircpStatus.ESStatus;
                summary.IRCPEYDStatus = ircpStatus.EYDStatus;
                summary.IRCPPanelStatus = ircpStatus.PanelStatus;

                var frcpStatus = GetFRCPStatus(user.Id);
                summary.FRCPESStatus = frcpStatus.ESStatus;
                summary.FRCPEYDStatus = frcpStatus.EYDStatus;
                summary.FRCPPanelStatus = frcpStatus.PanelStatus;

                // Update all status indicators
                summary.UpdateStatuses();

                summaries.Add(summary);
            }

            return summaries;
        }

        private (string ESStatus, string EYDStatus, string PanelStatus) GetIRCPStatus(string userId)
        {
            Console.WriteLine($"DEBUG TPD IRCP Status for {userId}:");
            
            // Check TempData locks first (TempData-first approach to match EYD controller)
            bool esLocked = TempData[$"IRCP_{userId}_ES_Locked"]?.ToString() == "true";
            bool eydLocked = TempData[$"IRCP_{userId}_EYD_Locked"]?.ToString() == "true";
            bool panelLocked = TempData[$"IRCP_{userId}_Panel_Locked"]?.ToString() == "true";
            
            Console.WriteLine($"  TempData ES_Locked: {TempData[$"IRCP_{userId}_ES_Locked"]}");
            Console.WriteLine($"  TempData EYD_Locked: {TempData[$"IRCP_{userId}_EYD_Locked"]}");
            Console.WriteLine($"  TempData Panel_Locked: {TempData[$"IRCP_{userId}_Panel_Locked"]}");

            // Initialize status
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

            // If not locked in TempData, check database for lock status and use database values
            if (!esLocked || !eydLocked || !panelLocked)
            {
                var ircpReview = _context.IRCPReviews
                    .FirstOrDefault(r => r.EYDUserId == userId);

                if (ircpReview != null)
                {
                    Console.WriteLine($"  DB ESLocked: {ircpReview.ESLocked}, ESStatus: {ircpReview.ESStatus}");
                    Console.WriteLine($"  DB EYDLocked: {ircpReview.EYDLocked}, EYDStatus: {ircpReview.EYDStatus}");
                    Console.WriteLine($"  DB PanelLocked: {ircpReview.PanelLocked}, PanelStatus: {ircpReview.PanelStatus}");

                    // Only update status if not already locked in TempData
                    if (!esLocked)
                    {
                        esStatus = ircpReview.ESLocked ? "Completed" : ircpReview.ESStatus.ToString();
                    }
                    if (!eydLocked)
                    {
                        eydStatus = ircpReview.EYDLocked ? "Completed" : ircpReview.EYDStatus.ToString();
                    }
                    if (!panelLocked)
                    {
                        panelStatus = ircpReview.PanelLocked ? "Completed" : ircpReview.PanelStatus.ToString();
                    }
                }
            }

            Console.WriteLine($"  Final locks: ES={esLocked}, EYD={eydLocked}, Panel={panelLocked}");
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

        private string CalculateStatusColor(int completed, int total)
        {
            if (total == 0) return "not-started";
            
            var percentage = (completed * 100.0) / total;
            
            if (percentage >= 80) return "complete";
            if (percentage >= 30) return "in-progress";
            return "not-started";
        }

        private string GetReviewStatus(string? stage)
        {
            return stage switch
            {
                "Completed" => "Complete",
                "Panel Review" => "Under Review",
                "Educational Supervisor Review" => "In Progress",
                "Draft" => "In Progress",
                null => "Not Started",
                "" => "Not Started",
                _ => "Not Started"
            };
        }
    }

    public class TPDDashboardViewModel
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string AssignedArea { get; set; } = "";
        public string AssignedScheme { get; set; } = "";
        public List<Scheme> ManagedSchemes { get; set; } = new List<Scheme>();
        public List<Scheme> AllAreaSchemes { get; set; } = new List<Scheme>(); // All schemes in the area
        public List<ApplicationUser> AssignedEYDUsers { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> EYDUsers { get; set; } = new List<ApplicationUser>(); // Alias for compatibility
        public List<SLE> PendingInvitations { get; set; } = new List<SLE>(); // Assessment invitations for this TPD
        public int? CurrentSchemeId { get; set; } // Track currently selected scheme
        
        // Enhanced Portfolio Summary Data
        public List<EYDPortfolioSummary> EYDPortfolioSummaries { get; set; } = new List<EYDPortfolioSummary>();
    }

    public class SchemeProgressViewModel
    {
        public Scheme Scheme { get; set; }
        public List<ApplicationUser> AssignedEYDUsers { get; set; } = new List<ApplicationUser>();
        public string TPDName { get; set; }
    }
}
