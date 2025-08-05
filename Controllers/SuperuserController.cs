using EYDGateway.Data;
using EYDGateway.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class SuperuserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperuserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            var areas = await _context.Areas.Include(a => a.Schemes).ToListAsync();
            var users = await _userManager.Users.Include(u => u.Area).Include(u => u.Scheme).ToListAsync();
            
            // Enhanced dashboard data
            var viewModel = new SuperuserDashboardViewModel
            {
                Areas = areas,
                Users = users
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AssignAdmin(int areaId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            var area = await _context.Areas.FindAsync(areaId);
            if (area == null)
            {
                return NotFound();
            }

            // ENHANCED: Get ALL Admin users, including those already assigned
            var availableAdmins = await _context.Users
                .Where(u => u.Role == "Admin")
                .ToListAsync();

            var viewModel = new AssignAdminViewModel
            {
                Area = area,
                AvailableAdmins = availableAdmins
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignAdmin(int areaId, string adminUserId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            var area = await _context.Areas.FindAsync(areaId);
            var admin = await _context.Users.FindAsync(adminUserId);

            if (area == null || admin == null || admin.Role != "Admin")
            {
                TempData["ErrorMessage"] = "Invalid area or admin selection.";
                return RedirectToAction("Dashboard");
            }

            // ENHANCED: Allow multiple admins per area - no restriction
            admin.AreaId = areaId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Admin '{admin.DisplayName}' assigned to area '{area.Name}' successfully.";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> ManageUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser" && currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            List<ApplicationUser> managableUsers;
            
            // Superuser can manage ALL users, Admin can only manage TPD, Dean, ES, EYD
            if (currentUser.Role == "Superuser")
            {
                managableUsers = await _context.Users
                    .Include(u => u.Area)
                    .Include(u => u.Scheme)
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.DisplayName)
                    .ToListAsync();
            }
            else // Admin
            {
                managableUsers = await _context.Users
                    .Where(u => u.Role == "TPD" || u.Role == "Dean" || u.Role == "ES" || u.Role == "EYD")
                    .Include(u => u.Area)
                    .Include(u => u.Scheme)
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.DisplayName)
                    .ToListAsync();
            }

            var areas = await _context.Areas.ToListAsync();
            var schemes = await _context.Schemes.Include(s => s.Area).ToListAsync();

            var viewModel = new SuperuserUserManagementViewModel
            {
                Users = managableUsers,
                Areas = areas,
                Schemes = schemes
            };

            ViewBag.CurrentUserRole = currentUser.Role;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReassignUser(string userId, string newRole, int? newAreaId, int? newSchemeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser" && currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            // Admin users cannot modify Admin or Superuser accounts
            if (currentUser.Role == "Admin" && (user.Role == "Admin" || user.Role == "Superuser"))
            {
                TempData["ErrorMessage"] = "You don't have permission to modify this user.";
                return RedirectToAction("ManageUsers");
            }

            // Update role if provided
            if (!string.IsNullOrEmpty(newRole))
            {
                // Admin users can only assign certain roles
                if (currentUser.Role == "Admin" && (newRole == "Admin" || newRole == "Superuser"))
                {
                    TempData["ErrorMessage"] = "You don't have permission to assign this role.";
                    return RedirectToAction("ManageUsers");
                }
                
                user.Role = newRole;
            }

            // Apply assignment logic based on role
            switch (user.Role)
            {
                case "Admin":
                    user.AreaId = newAreaId;
                    user.SchemeId = null;
                    break;
                case "TPD":
                case "EYD":
                    user.SchemeId = newSchemeId;
                    user.AreaId = null;
                    break;
                case "ES":
                    user.AreaId = newAreaId;
                    user.SchemeId = null;
                    break;
                case "Dean":
                case "Superuser":
                    user.AreaId = null;
                    user.SchemeId = null;
                    break;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User '{user.DisplayName}' updated successfully.";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserAssignment(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser" && currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            // Admin users cannot modify Admin or Superuser accounts
            if (currentUser.Role == "Admin" && (user.Role == "Admin" || user.Role == "Superuser"))
            {
                TempData["ErrorMessage"] = "You don't have permission to modify this user.";
                return RedirectToAction("ManageUsers");
            }

            user.AreaId = null;
            user.SchemeId = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"All assignments removed for user '{user.DisplayName}'.";
            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> ManageSchemes()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            var areas = await _context.Areas.ToListAsync();
            var schemes = await _context.Schemes
                .Include(s => s.Area)
                .OrderBy(s => s.Area.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();
            var users = await _context.Users
                .Include(u => u.Area)
                .Include(u => u.Scheme)
                .ToListAsync();

            var viewModel = new SuperuserSchemeViewModel
            {
                Areas = areas,
                Schemes = schemes,
                Users = users
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchemeInArea(string schemeName, int areaId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            if (!string.IsNullOrWhiteSpace(schemeName) && areaId > 0)
            {
                var area = await _context.Areas.FindAsync(areaId);
                if (area != null)
                {
                    var scheme = new Scheme
                    {
                        Name = schemeName,
                        AreaId = areaId
                    };
                    
                    _context.Schemes.Add(scheme);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Scheme '{schemeName}' created in area '{area.Name}' successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Area not found.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Scheme name and area are required.";
            }
            
            return RedirectToAction("ManageSchemes");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteScheme(int schemeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            var scheme = await _context.Schemes.FindAsync(schemeId);
            if (scheme != null)
            {
                // Remove user assignments first
                var usersInScheme = await _context.Users
                    .Where(u => u.SchemeId == schemeId)
                    .ToListAsync();

                foreach (var user in usersInScheme)
                {
                    user.SchemeId = null;
                }

                // Remove ES-EYD assignments
                var assignments = await _context.EYDESAssignments
                    .Where(a => a.EYDUser.SchemeId == schemeId)
                    .ToListAsync();

                _context.EYDESAssignments.RemoveRange(assignments);

                _context.Schemes.Remove(scheme);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Scheme '{scheme.Name}' deleted and user assignments removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Scheme not found.";
            }
            
            return RedirectToAction("ManageSchemes");
        }

        public async Task<IActionResult> CreateNewUser()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser" && currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var areas = await _context.Areas.ToListAsync();
            var schemes = await _context.Schemes.Include(s => s.Area).ToListAsync();
            
            List<ApplicationUser> existingUsers;
            if (currentUser.Role == "Superuser")
            {
                existingUsers = await _context.Users
                    .Include(u => u.Area)
                    .Include(u => u.Scheme)
                    .ToListAsync();
            }
            else // Admin
            {
                existingUsers = await _context.Users
                    .Where(u => u.Role == "TPD" || u.Role == "Dean" || u.Role == "ES" || u.Role == "EYD")
                    .Include(u => u.Area)
                    .Include(u => u.Scheme)
                    .ToListAsync();
            }
            
            var viewModel = new SuperuserCreateUserViewModel
            {
                Areas = areas,
                Schemes = schemes,
                ExistingUsers = existingUsers
            };
            
            ViewBag.CurrentUserRole = currentUser.Role;
            return View(viewModel);
        }

        public async Task<IActionResult> EditUserDetails(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser" && currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            // Admin users cannot edit Admin or Superuser accounts
            if (currentUser.Role == "Admin" && (user.Role == "Admin" || user.Role == "Superuser"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this user's details.";
                return RedirectToAction("ManageUsers");
            }

            // Split display name into first and last name (if possible)
            var nameParts = user.DisplayName?.Split(' ', 2) ?? new[] { "", "" };
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var viewModel = new EditUserDetailsViewModel
            {
                UserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName ?? "",
                CurrentRole = user.Role ?? ""
            };

            // Pass available roles to view
            ViewBag.AvailableRoles = currentUser.Role == "Superuser" 
                ? new[] { "ES", "TPD", "Dean", "EYD", "Admin", "Superuser" }
                : new[] { "ES", "TPD", "Dean", "EYD" }; // Admin can't assign Admin/Superuser roles
            
            ViewBag.CurrentUserRole = currentUser.Role;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserDetails(EditUserDetailsViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser" && currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            // Admin users cannot edit Admin or Superuser accounts
            if (currentUser.Role == "Admin" && (user.Role == "Admin" || user.Role == "Superuser"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this user's details.";
                return RedirectToAction("ManageUsers");
            }

            // Validation
            if (model.ChangePassword)
            {
                if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword.Length < 6)
                {
                    ModelState.AddModelError("NewPassword", "Password must be at least 6 characters long.");
                }
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                }
            }

            if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
            {
                ModelState.AddModelError("Email", "Valid email address is required.");
            }

            if (ModelState.IsValid)
            {
                // Update user details
                user.Email = model.Email;
                user.UserName = model.Email; // Username typically matches email
                user.DisplayName = !string.IsNullOrEmpty(model.DisplayName) 
                    ? model.DisplayName 
                    : $"{model.FirstName} {model.LastName}".Trim();

                // Handle role change if different
                if (!string.IsNullOrEmpty(model.CurrentRole) && model.CurrentRole != user.Role)
                {
                    // Remove user from old role if they had one
                    if (!string.IsNullOrEmpty(user.Role))
                    {
                        await _userManager.RemoveFromRoleAsync(user, user.Role);
                    }
                    
                    // Update role field
                    user.Role = model.CurrentRole;
                    
                    // Add user to new Identity role
                    await _userManager.AddToRoleAsync(user, model.CurrentRole);
                }

                // Update password if requested
                if (model.ChangePassword && !string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        ViewBag.CurrentUserRole = currentUser.Role;
                        return View(model);
                    }
                }

                // Save changes
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    TempData["SuccessMessage"] = $"User details for '{user.DisplayName}' updated successfully.";
                    return RedirectToAction("ManageUsers");
                }
                else
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            ViewBag.CurrentUserRole = currentUser.Role;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewUser(SuperuserCreateUserViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser" && currentUser?.Role != "Admin")
            {
                return Unauthorized();
            }

            // Admin users cannot create Admin or Superuser accounts
            if (currentUser.Role == "Admin" && (model.Role == "Admin" || model.Role == "Superuser"))
            {
                ModelState.AddModelError("Role", "You don't have permission to create this type of user.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    DisplayName = $"{model.FirstName} {model.LastName}",
                    Role = model.Role
                };

                // Apply assignment logic
                switch (model.Role)
                {
                    case "Admin":
                        user.AreaId = model.AreaId;
                        user.SchemeId = null;
                        break;
                    case "TPD":
                    case "EYD":
                        user.SchemeId = model.SchemeId;
                        user.AreaId = null;
                        break;
                    case "ES":
                        user.AreaId = model.AreaId;
                        user.SchemeId = null;
                        break;
                    case "Dean":
                    case "Superuser":
                        user.AreaId = null;
                        user.SchemeId = null;
                        break;
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Add user to the selected Identity role
                    await _userManager.AddToRoleAsync(user, model.Role);
                    
                    TempData["SuccessMessage"] = $"User '{model.FirstName} {model.LastName}' created successfully.";
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Reload data for view
            var areas = await _context.Areas.ToListAsync();
            var schemes = await _context.Schemes.Include(s => s.Area).ToListAsync();
            
            List<ApplicationUser> existingUsers;
            if (currentUser.Role == "Superuser")
            {
                existingUsers = await _context.Users
                    .Include(u => u.Area)
                    .Include(u => u.Scheme)
                    .ToListAsync();
            }
            else // Admin
            {
                existingUsers = await _context.Users
                    .Where(u => u.Role == "TPD" || u.Role == "Dean" || u.Role == "ES" || u.Role == "EYD")
                    .Include(u => u.Area)
                    .Include(u => u.Scheme)
                    .ToListAsync();
            }
            
            model.Areas = areas;
            model.Schemes = schemes;
            model.ExistingUsers = existingUsers;
            
            ViewBag.CurrentUserRole = currentUser.Role;
            return View(model);
        }

        public async Task<IActionResult> CreateArea()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateArea(string areaName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
            {
                return Unauthorized();
            }

            if (!string.IsNullOrWhiteSpace(areaName))
            {
                var area = new Area { Name = areaName };
                _context.Areas.Add(area);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Area '{areaName}' created successfully.";
                return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError("", "Area name is required.");
            return View();
        }

        public async Task<IActionResult> SystemOverview()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "Superuser")
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

            var systemOverview = new SystemOverviewViewModel
            {
                TotalUsers = allUsers.Count,
                UsersByRole = allUsers.GroupBy(u => u.Role ?? "Unassigned")
                    .ToDictionary(g => g.Key, g => g.Count()),
                TotalAreas = allAreas.Count,
                TotalSchemes = allAreas.SelectMany(a => a.Schemes).Count(),
                UnassignedAdmins = allUsers.Count(u => u.Role == "Admin" && u.AreaId == null),
                AreasWithoutAdmins = allAreas.Count(a => !allUsers.Any(u => u.Role == "Admin" && u.AreaId == a.Id)),
                RecentUsers = allUsers.OrderByDescending(u => u.Id).Take(10).ToList()
            };

            return View(systemOverview);
        }

        // Test action to verify controller is working
        public IActionResult Test()
        {
            return Content("Superuser controller is working!");
        }
    }
}
