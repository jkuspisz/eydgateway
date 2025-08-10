using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class ESController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ESController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Redirect to user-specific dashboard
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.Role != "ES")
            {
                return Unauthorized();
            }

            return RedirectToAction("UserDashboard", new { userId = currentUser.Id });
        }

        public async Task<IActionResult> UserDashboard(string? userId = null)
        {
            var currentUser = await _context.Users
                .Include(u => u.Area)
                    .ThenInclude(a => a!.Schemes)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.Role != "ES")
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

            // Get EYD users assigned to this ES through EYDESAssignment table
            var assignedEYDUsers = await _context.EYDESAssignments
                .Where(assignment => assignment.ESUserId == currentUser.Id && assignment.IsActive)
                .Include(assignment => assignment.EYDUser)
                    .ThenInclude(eyd => eyd.Scheme)
                        .ThenInclude(scheme => scheme!.Area)
                .Select(assignment => assignment.EYDUser)
                .ToListAsync();

            // Get pending assessment invitations for this ES user
            var pendingInvitations = await _context.SLEs
                .Where(sle => sle.AssessorUserId == currentUser.Id &&
                              sle.Status == "Invited" &&
                              !sle.IsAssessmentCompleted)
                .Include(sle => sle.EYDUser)
                .ToListAsync();

            // Since EPAMappings are polymorphic (EntityType/EntityId), EF can't auto-include them via navigation.
            // Manually load mappings for these SLEs and attach them for the view to render EPA codes.
            if (pendingInvitations.Count > 0)
            {
                var sleIds = pendingInvitations.Select(pi => pi.Id).ToList();
                var mappings = await _context.EPAMappings
                    .Where(m => m.EntityType == "SLE" && sleIds.Contains(m.EntityId))
                    .Include(m => m.EPA)
                    .ToListAsync();

                var mappingsBySle = mappings
                    .GroupBy(m => m.EntityId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var sle in pendingInvitations)
                {
                    if (mappingsBySle.TryGetValue(sle.Id, out var sleMappings))
                    {
                        sle.EPAMappings = sleMappings;
                    }
                    else
                    {
                        // Ensure it's an empty collection rather than null for safe view rendering
                        sle.EPAMappings = new List<EPAMapping>();
                    }
                }

                // DEBUG
                Console.WriteLine($"DEBUG ES Dashboard: Loaded {mappings.Count} EPA mappings for {pendingInvitations.Count} pending invitations.");
            }

            var viewModel = new ESDashboardViewModel
            {
                UserId = currentUser.Id,
                UserName = currentUser.DisplayName ?? currentUser.UserName ?? "Unknown User",
                AssignedArea = currentUser.Area?.Name ?? "No Area Assigned",
                ManagedSchemes = currentUser.Area?.Schemes?.ToList() ?? new List<Scheme>(),
                TPDUsers = new List<ApplicationUser>(), // ES doesn't need to see TPD users anymore
                EYDUsers = assignedEYDUsers,
                PendingInvitations = pendingInvitations // Individual assessment tasks for this ES
            };

            // DEBUG: Log the EYD users being passed to the view
            Console.WriteLine($"DEBUG ES Dashboard: Found {assignedEYDUsers.Count} assigned EYD users for {currentUser.UserName}");
            foreach (var eyd in assignedEYDUsers)
            {
                Console.WriteLine($"DEBUG ES Dashboard: EYD User - ID: '{eyd.Id}', Name: '{eyd.DisplayName}', Email: '{eyd.Email}'");
            }

            return View(viewModel);
        }

        public async Task<IActionResult> ViewAreaProgress()
        {
            var currentUser = await _context.Users
                .Include(u => u.Area)
                    .ThenInclude(a => a!.Schemes)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.Role != "ES")
            {
                return Unauthorized();
            }

            var allUsersInArea = await _context.Users
                .Where(u => u.AreaId == currentUser.AreaId)
                .Include(u => u.Area)
                .ToListAsync();

            var viewModel = new AreaProgressViewModel
            {
                Area = currentUser.Area,
                ESName = currentUser.DisplayName ?? currentUser.UserName ?? string.Empty,
                AllUsers = allUsersInArea,
                Schemes = currentUser.Area?.Schemes?.ToList() ?? new List<Scheme>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignTPDToScheme(string tpdUserId, int schemeId)
        {
            var tpdUser = await _context.Users.FindAsync(tpdUserId);
            var scheme = await _context.Schemes
                .Include(s => s.Area)
                .FirstOrDefaultAsync(s => s.Id == schemeId);

            if (tpdUser == null || scheme == null)
            {
                TempData["ErrorMessage"] = "Invalid user or scheme selection.";
                return RedirectToAction("Dashboard");
            }

            // Verify the ES manages this area
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.AreaId != scheme.AreaId)
            {
                return Unauthorized();
            }

            // Update TPD user's area assignment (they can manage this scheme)
            tpdUser.AreaId = scheme.AreaId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"TPD {tpdUser.DisplayName} assigned to manage {scheme.Name} successfully.";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> GenerateReport()
        {
            var currentUser = await _context.Users
                .Include(u => u.Area)
                    .ThenInclude(a => a!.Schemes)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser?.Role != "ES")
            {
                return Unauthorized();
            }

            // Generate a comprehensive report for the area
            var reportData = new AreaReportViewModel
            {
                Area = currentUser.Area,
                GeneratedBy = currentUser.DisplayName ?? currentUser.UserName ?? string.Empty,
                GeneratedDate = DateTime.Now,
                TotalSchemes = currentUser.Area?.Schemes?.Count ?? 0,
                CompletedSchemes = 2, // Mock data
                InProgressSchemes = 1, // Mock data
                TotalUsers = await _context.Users.CountAsync(u => u.AreaId == currentUser.AreaId)
            };

            return View(reportData);
        }
    }

    public class ESDashboardViewModel
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string AssignedArea { get; set; } = "";
        public List<Scheme> ManagedSchemes { get; set; } = new List<Scheme>();
        public List<ApplicationUser> TPDUsers { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> EYDUsers { get; set; } = new List<ApplicationUser>();
        public List<SLE> PendingInvitations { get; set; } = new List<SLE>(); // Assessment invitations for this ES
    }

    public class AreaProgressViewModel
    {
    public Area? Area { get; set; }
    public string ESName { get; set; } = string.Empty;
        public List<ApplicationUser> AllUsers { get; set; } = new List<ApplicationUser>();
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
    }

    public class AreaReportViewModel
    {
    public Area? Area { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public int TotalSchemes { get; set; }
        public int CompletedSchemes { get; set; }
        public int InProgressSchemes { get; set; }
        public int TotalUsers { get; set; }
    }
}
