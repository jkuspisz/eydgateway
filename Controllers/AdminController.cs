using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var allUsers = await _context.Users
                .Include(u => u.Area)
                .Include(u => u.Scheme)
                .ToListAsync();

            var allAreas = await _context.Areas
                .Include(a => a.Schemes)
                .ToListAsync();

            // Get Admin's assigned area schemes
            var adminAreaSchemes = currentUser.AreaId.HasValue 
                ? await _context.Schemes
                    .Where(s => s.AreaId == currentUser.AreaId.Value)
                    .Include(s => s.Area)
                    .ToListAsync()
                : new List<Scheme>();

            // Get users within Admin's area
            var usersInArea = currentUser.AreaId.HasValue
                ? allUsers.Where(u => u.AreaId == currentUser.AreaId || 
                                     (u.Scheme != null && u.Scheme.AreaId == currentUser.AreaId))
                          .ToList()
                : allUsers;

            var viewModel = new AdminDashboardViewModel
            {
                UserName = currentUser.DisplayName ?? currentUser.UserName,
                AdminArea = currentUser.Area?.Name ?? "No Area Assigned",
                TotalUsers = allUsers.Count,
                UsersInMyArea = usersInArea.Count,
                TotalAreas = allAreas.Count,
                TotalSchemes = allAreas.SelectMany(a => a.Schemes).Count(),
                SchemesInMyArea = adminAreaSchemes.Count,
                UsersByRole = allUsers.GroupBy(u => u.Role ?? "Unassigned")
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentUsers = allUsers.OrderByDescending(u => u.Id).Take(5).ToList(),
                Areas = allAreas,
                MyAreaSchemes = adminAreaSchemes,
                UsersInMyAreaByRole = usersInArea.GroupBy(u => u.Role ?? "Unassigned")
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CreateUser()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            var areas = await _context.Areas.ToListAsync();
            ViewBag.Areas = areas;
            
            // Get schemes within Admin's area for TPD/EYD assignment
            var schemes = currentUser?.AreaId.HasValue == true
                ? await _context.Schemes
                    .Where(s => s.AreaId == currentUser.AreaId.Value)
                    .ToListAsync()
                : new List<Scheme>();
            
            ViewBag.Schemes = schemes;
            ViewBag.Roles = new[] { "Admin", "Superuser", "ES", "TPD", "Dean", "EYD" };
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    DisplayName = model.DisplayName,
                    Role = model.Role
                };

                // PHASE 2.2: Enhanced assignment logic
                if (model.Role == "Admin")
                {
                    user.AreaId = model.AreaId > 0 ? model.AreaId : null;
                    user.SchemeId = null;
                }
                else if (model.Role == "TPD" || model.Role == "EYD")
                {
                    user.SchemeId = model.SchemeId > 0 ? model.SchemeId : null;
                    user.AreaId = null; // TPDs and EYDs are assigned to schemes, not areas
                }
                else if (model.Role == "ES")
                {
                    user.AreaId = model.AreaId > 0 ? model.AreaId : null;
                    user.SchemeId = null; // ES users assigned to areas for geographic context
                }
                else // Dean, Superuser
                {
                    user.AreaId = null;
                    user.SchemeId = null;
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"User {model.DisplayName} created successfully.";
                    return RedirectToAction("Dashboard");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var areas = await _context.Areas.ToListAsync();
            ViewBag.Areas = areas;
            
            var schemes = currentUser?.AreaId.HasValue == true
                ? await _context.Schemes
                    .Where(s => s.AreaId == currentUser.AreaId.Value)
                    .ToListAsync()
                : new List<Scheme>();
            
            ViewBag.Schemes = schemes;
            ViewBag.Roles = new[] { "Admin", "Superuser", "ES", "TPD", "Dean", "EYD" };
            return View(model);
        }

        // Redirect Admin users to the shared user management interface
        public async Task<IActionResult> ManageUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            // Redirect to SuperuserController's ManageUsers which now handles both Superuser and Admin roles
            return RedirectToAction("ManageUsers", "Superuser");
        }

        [HttpPost]
        public async Task<IActionResult> ReassignUser(string userId, string newRole, int? newAreaId, int? newSchemeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            // Redirect to SuperuserController's ReassignUser which now handles both Superuser and Admin roles
            return RedirectToAction("ReassignUser", "Superuser", new { userId, newRole, newAreaId, newSchemeId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserAssignment(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            // Redirect to SuperuserController's RemoveUserAssignment which now handles both Superuser and Admin roles
            return RedirectToAction("RemoveUserAssignment", "Superuser", new { userId });
        }

        public async Task<IActionResult> CreateNewUser()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            // Redirect to SuperuserController's CreateNewUser which now handles both Superuser and Admin roles
            return RedirectToAction("CreateNewUser", "Superuser");
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewUser([FromForm] CreateUserViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            // Redirect to SuperuserController's CreateNewUser POST which now handles both Superuser and Admin roles
            return RedirectToAction("CreateNewUser", "Superuser", model);
        }

        public async Task<IActionResult> ManageAreas()
        {
            var areas = await _context.Areas
                .Include(a => a.Schemes)
                .ToListAsync();

            return View(areas);
        }

        [HttpPost]
        public async Task<IActionResult> CreateArea(string areaName)
        {
            if (!string.IsNullOrWhiteSpace(areaName))
            {
                var area = new Area { Name = areaName };
                _context.Areas.Add(area);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Area '{areaName}' created successfully.";
            }
            
            return RedirectToAction("ManageAreas");
        }

        [HttpPost]
        public async Task<IActionResult> CreateScheme(string schemeName, int areaId)
        {
            if (!string.IsNullOrWhiteSpace(schemeName) && areaId > 0)
            {
                var scheme = new Scheme 
                { 
                    Name = schemeName,
                    AreaId = areaId
                };
                _context.Schemes.Add(scheme);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Scheme '{schemeName}' created successfully.";
            }
            
            return RedirectToAction("ManageAreas");
        }

        // NEW PHASE 2.2 METHODS - Enhanced Scheme Management

        public async Task<IActionResult> ManageSchemes()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin" || !currentUser.AreaId.HasValue)
            {
                return Unauthorized();
            }

            var schemes = await _context.Schemes
                .Where(s => s.AreaId == currentUser.AreaId.Value)
                .Include(s => s.Area)
                .ToListAsync();

            var schemeViewModels = new List<SchemeManagementViewModel>();

            foreach (var scheme in schemes)
            {
                var tpdUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.SchemeId == scheme.Id && u.Role == "TPD");
                
                var eydUsers = await _context.Users
                    .Where(u => u.SchemeId == scheme.Id && u.Role == "EYD")
                    .ToListAsync();

                schemeViewModels.Add(new SchemeManagementViewModel
                {
                    Scheme = scheme,
                    AssignedTPD = tpdUser,
                    AssignedEYDs = eydUsers,
                    EYDCount = eydUsers.Count
                });
            }

            ViewBag.AdminArea = currentUser.Area?.Name;
            ViewBag.AreaId = currentUser.AreaId;
            return View(schemeViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> CreateYearlyScheme(string baseName, int year, int areaId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin" || currentUser.AreaId != areaId)
            {
                return Unauthorized();
            }

            if (!string.IsNullOrWhiteSpace(baseName) && year > 0)
            {
                var yearlyScheme = new Scheme
                {
                    Name = $"{baseName} {year}",
                    AreaId = areaId
                };
                
                _context.Schemes.Add(yearlyScheme);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Yearly scheme '{yearlyScheme.Name}' created successfully.";
            }
            
            return RedirectToAction("ManageSchemes");
        }

        [HttpPost]
        public async Task<IActionResult> EditScheme(int schemeId, string newName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var scheme = await _context.Schemes
                .FirstOrDefaultAsync(s => s.Id == schemeId && s.AreaId == currentUser.AreaId);

            if (scheme != null && !string.IsNullOrWhiteSpace(newName))
            {
                scheme.Name = newName;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Scheme updated to '{newName}' successfully.";
            }
            
            return RedirectToAction("ManageSchemes");
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveScheme(int schemeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var scheme = await _context.Schemes
                .FirstOrDefaultAsync(s => s.Id == schemeId && s.AreaId == currentUser.AreaId);

            if (scheme != null)
            {
                // Before archiving, remove user assignments
                var usersInScheme = await _context.Users
                    .Where(u => u.SchemeId == schemeId)
                    .ToListAsync();

                foreach (var user in usersInScheme)
                {
                    user.SchemeId = null;
                }

                _context.Schemes.Remove(scheme);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Scheme '{scheme.Name}' archived and user assignments removed.";
            }
            
            return RedirectToAction("ManageSchemes");
        }

        public async Task<IActionResult> AssignUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin" || !currentUser.AreaId.HasValue)
            {
                return Unauthorized();
            }

            var schemes = await _context.Schemes
                .Where(s => s.AreaId == currentUser.AreaId.Value)
                .Include(s => s.Area)
                .ToListAsync();

            var tpdUsers = await _context.Users
                .Where(u => u.Role == "TPD" && u.SchemeId == null)
                .ToListAsync();

            var eydUsers = await _context.Users
                .Where(u => u.Role == "EYD" && u.SchemeId == null)
                .ToListAsync();

            var esUsers = await _context.Users
                .Where(u => u.Role == "ES")
                .ToListAsync();

            var assignedEydUsers = await _context.Users
                .Where(u => u.Role == "EYD" && u.Scheme != null && u.Scheme.AreaId == currentUser.AreaId.Value)
                .Include(u => u.Scheme)
                .ToListAsync();

            var viewModel = new UserAssignmentViewModel
            {
                Schemes = schemes,
                UnassignedTPDs = tpdUsers,
                UnassignedEYDs = eydUsers,
                ESUsers = esUsers,
                AssignedEYDs = assignedEydUsers
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignTPDToScheme(string userId, int schemeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            var scheme = await _context.Schemes
                .FirstOrDefaultAsync(s => s.Id == schemeId && s.AreaId == currentUser.AreaId);

            if (user != null && scheme != null && user.Role == "TPD")
            {
                // Check if scheme already has a TPD
                var existingTPD = await _context.Users
                    .FirstOrDefaultAsync(u => u.SchemeId == schemeId && u.Role == "TPD");

                if (existingTPD != null)
                {
                    TempData["ErrorMessage"] = $"Scheme '{scheme.Name}' already has a TPD assigned.";
                }
                else
                {
                    user.SchemeId = schemeId;
                    user.AreaId = null; // TPDs are assigned to schemes, not areas
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"TPD '{user.DisplayName}' assigned to scheme '{scheme.Name}'.";
                }
            }
            
            return RedirectToAction("AssignUsers");
        }

        [HttpPost]
        public async Task<IActionResult> AssignEYDToScheme(string userId, int schemeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            var scheme = await _context.Schemes
                .FirstOrDefaultAsync(s => s.Id == schemeId && s.AreaId == currentUser.AreaId);

            if (user != null && scheme != null && user.Role == "EYD")
            {
                user.SchemeId = schemeId;
                user.AreaId = null; // EYDs are assigned to schemes, not areas
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"EYD '{user.DisplayName}' assigned to scheme '{scheme.Name}'.";
            }
            
            return RedirectToAction("AssignUsers");
        }

        [HttpPost]
        public async Task<IActionResult> AssignESToEYD(string esUserId, string eydUserId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var esUser = await _context.Users.FindAsync(esUserId);
            var eydUser = await _context.Users
                .Include(u => u.Scheme)
                .FirstOrDefaultAsync(u => u.Id == eydUserId);

            if (esUser?.Role == "ES" && eydUser?.Role == "EYD" && 
                eydUser.Scheme?.AreaId == currentUser.AreaId)
            {
                // Check if assignment already exists
                var existingAssignment = await _context.EYDESAssignments
                    .FirstOrDefaultAsync(a => a.ESUserId == esUserId && a.EYDUserId == eydUserId && a.IsActive);

                if (existingAssignment == null)
                {
                    var assignment = new EYDESAssignment
                    {
                        ESUserId = esUserId,
                        EYDUserId = eydUserId,
                        AssignedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.EYDESAssignments.Add(assignment);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"ES '{esUser.DisplayName}' assigned to EYD '{eydUser.DisplayName}'.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"ES '{esUser.DisplayName}' is already assigned to EYD '{eydUser.DisplayName}'.";
                }
            }
            
            return RedirectToAction("AssignUsers");
        }

        public async Task<IActionResult> ManageESAssignments()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin" || !currentUser.AreaId.HasValue)
            {
                return Unauthorized();
            }

            var assignments = await _context.EYDESAssignments
                .Where(a => a.IsActive && a.EYDUser.Scheme!.AreaId == currentUser.AreaId.Value)
                .Include(a => a.ESUser)
                .Include(a => a.EYDUser)
                .ThenInclude(u => u.Scheme)
                .ToListAsync();

            return View(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveESAssignment(int assignmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var assignment = await _context.EYDESAssignments
                .Include(a => a.EYDUser)
                .ThenInclude(u => u.Scheme)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && 
                                        a.EYDUser.Scheme!.AreaId == currentUser.AreaId);

            if (assignment != null)
            {
                assignment.IsActive = false;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "ES assignment removed successfully.";
            }
            
            return RedirectToAction("ManageESAssignments");
        }

        public async Task<IActionResult> SystemReport()
        {
            var allUsers = await _context.Users
                .Include(u => u.Area)
                .ToListAsync();

            var allAreas = await _context.Areas
                .Include(a => a.Schemes)
                .ToListAsync();

            var reportModel = new SystemReportViewModel
            {
                GeneratedBy = (await _userManager.GetUserAsync(User))?.DisplayName ?? "Admin",
                GeneratedDate = DateTime.Now,
                TotalUsers = allUsers.Count,
                UsersByRole = allUsers.GroupBy(u => u.Role ?? "Unassigned")
                    .ToDictionary(g => g.Key, g => g.Count()),
                TotalAreas = allAreas.Count,
                TotalSchemes = allAreas.SelectMany(a => a.Schemes).Count(),
                AreaDetails = allAreas.Select(a => new AreaDetailViewModel
                {
                    Area = a,
                    UserCount = allUsers.Count(u => u.AreaId == a.Id),
                    SchemeCount = a.Schemes?.Count ?? 0
                }).ToList()
            };

            return View(reportModel);
        }

        // Test action to verify controller is working
        public IActionResult Test()
        {
            return Content("Admin controller is working!");
        }
    }

    public class AdminDashboardViewModel
    {
        public string UserName { get; set; } = "";
        public string AdminArea { get; set; } = "";
        public int TotalUsers { get; set; }
        public int UsersInMyArea { get; set; }
        public int TotalAreas { get; set; }
        public int TotalSchemes { get; set; }
        public int SchemesInMyArea { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UsersInMyAreaByRole { get; set; } = new Dictionary<string, int>();
        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
        public List<Area> Areas { get; set; } = new List<Area>();
        public List<Scheme> MyAreaSchemes { get; set; } = new List<Scheme>();
    }

    public class CreateUserViewModel
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public int? AreaId { get; set; }
        public int? SchemeId { get; set; }  // NEW: For TPD/EYD assignments
    }

    public class SystemReportViewModel
    {
        public string GeneratedBy { get; set; }
        public DateTime GeneratedDate { get; set; }
        public int TotalUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public int TotalAreas { get; set; }
        public int TotalSchemes { get; set; }
        public List<AreaDetailViewModel> AreaDetails { get; set; } = new List<AreaDetailViewModel>();
    }

    public class AreaDetailViewModel
    {
        public Area Area { get; set; } = new Area();
        public int UserCount { get; set; }
        public int SchemeCount { get; set; }
    }

    // NEW PHASE 2.2 VIEW MODELS

    public class SchemeManagementViewModel
    {
        public Scheme Scheme { get; set; } = new Scheme();
        public ApplicationUser? AssignedTPD { get; set; }
        public List<ApplicationUser> AssignedEYDs { get; set; } = new List<ApplicationUser>();
        public int EYDCount { get; set; }
    }

    public class UserAssignmentViewModel
    {
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
        public List<ApplicationUser> UnassignedTPDs { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> UnassignedEYDs { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> ESUsers { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> AssignedEYDs { get; set; } = new List<ApplicationUser>();
    }
}
