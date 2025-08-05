using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class TPDController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TPDController(ApplicationDbContext context)
        {
            _context = context;
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
                CurrentSchemeId = currentUser.SchemeId // Track the currently selected scheme
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

            var viewModel = new TPDDashboardViewModel
            {
                UserName = currentUser.DisplayName ?? currentUser.UserName,
                AssignedArea = currentUser.Scheme?.Area?.Name ?? "No Area Assigned",
                ManagedSchemes = allAreaSchemes,
                AssignedEYDUsers = schemeEYDs,
                CurrentSchemeId = schemeId
            };

            ViewBag.UserRole = currentUser.Role;
            ViewBag.SelectedSchemeName = scheme.Name;

            return View("Dashboard", viewModel);
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
    }

    public class SchemeProgressViewModel
    {
        public Scheme Scheme { get; set; }
        public List<ApplicationUser> AssignedEYDUsers { get; set; } = new List<ApplicationUser>();
        public string TPDName { get; set; }
    }
}
