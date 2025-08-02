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
            var currentUser = await _context.Users
                .Include(u => u.Area)
                    .ThenInclude(a => a.Schemes)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser?.Role != "ES")
            {
                return Unauthorized();
            }

            // Get EYD users assigned to this ES through EYDESAssignment table
            var assignedEYDUsers = await _context.EYDESAssignments
                .Where(assignment => assignment.ESUserId == currentUser.Id && assignment.IsActive)
                .Include(assignment => assignment.EYDUser)
                    .ThenInclude(eyd => eyd.Scheme)
                        .ThenInclude(scheme => scheme.Area)
                .Select(assignment => assignment.EYDUser)
                .ToListAsync();

            var viewModel = new ESDashboardViewModel
            {
                UserName = currentUser.DisplayName ?? currentUser.UserName,
                AssignedArea = currentUser.Area?.Name ?? "No Area Assigned",
                ManagedSchemes = currentUser.Area?.Schemes?.ToList() ?? new List<Scheme>(),
                TPDUsers = new List<ApplicationUser>(), // ES doesn't need to see TPD users anymore
                EYDUsers = assignedEYDUsers
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ViewAreaProgress()
        {
            var currentUser = await _context.Users
                .Include(u => u.Area)
                    .ThenInclude(a => a.Schemes)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

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
                ESName = currentUser.DisplayName ?? currentUser.UserName,
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
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

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
                    .ThenInclude(a => a.Schemes)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser?.Role != "ES")
            {
                return Unauthorized();
            }

            // Generate a comprehensive report for the area
            var reportData = new AreaReportViewModel
            {
                Area = currentUser.Area,
                GeneratedBy = currentUser.DisplayName ?? currentUser.UserName,
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
        public string UserName { get; set; }
        public string AssignedArea { get; set; }
        public List<Scheme> ManagedSchemes { get; set; } = new List<Scheme>();
        public List<ApplicationUser> TPDUsers { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> EYDUsers { get; set; } = new List<ApplicationUser>();
    }

    public class AreaProgressViewModel
    {
        public Area Area { get; set; }
        public string ESName { get; set; }
        public List<ApplicationUser> AllUsers { get; set; } = new List<ApplicationUser>();
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
    }

    public class AreaReportViewModel
    {
        public Area Area { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedDate { get; set; }
        public int TotalSchemes { get; set; }
        public int CompletedSchemes { get; set; }
        public int InProgressSchemes { get; set; }
        public int TotalUsers { get; set; }
    }
}
