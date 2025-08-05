using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class ReflectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReflectionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reflection
        public async Task<IActionResult> Index(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = id ?? currentUser.Id;

            // Authorization check for viewing reflections
            if (currentUser.Role == "EYD")
            {
                // EYD users can only view their own reflections
                if (targetUserId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role == "ES")
            {
                // ES users can view reflections of their assigned EYD users
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == targetUserId && 
                             assignment.IsActive);
                
                if (!isAssigned) return Forbid();
            }
            else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
            {
                return Forbid(); // Other roles are not allowed
            }

            var reflections = await _context.Reflections
                .Where(r => r.UserId == targetUserId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Explicitly load EPA mappings for each reflection
            foreach (var reflection in reflections)
            {
                var epaMappings = await _context.EPAMappings
                    .Include(em => em.EPA)
                    .Where(em => em.EntityType == "Reflection" && em.EntityId == reflection.Id)
                    .ToListAsync();
                reflection.EPAMappings = epaMappings;
            }

            // Get target user info for display
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            ViewBag.TargetUserName = targetUser?.UserName ?? "Unknown User";
            ViewBag.TargetUserId = targetUserId;
            
            // Check if current user can edit (only EYD users can edit their own reflections)
            ViewBag.CanEdit = currentUser.Role == "EYD" && targetUserId == currentUser.Id;

            return View(reflections);
        }

        // GET: Reflection/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get reflection without user restriction first
            var reflection = await _context.Reflections
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reflection == null) return NotFound();

            // Check authorization based on user role
            if (currentUser.Role == "EYD")
            {
                // EYD users can only view their own reflections
                if (reflection.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role == "ES")
            {
                // ES users can view reflections of their assigned EYD users
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == reflection.UserId && 
                             assignment.IsActive);
                
                if (!isAssigned) return Forbid();
            }
            else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
            {
                return Forbid(); // Other roles are not allowed
            }

            // Load EPA mappings for this reflection
            var epaMappings = await _context.EPAMappings
                .Include(em => em.EPA)
                .Where(em => em.EntityType == "Reflection" && em.EntityId == reflection.Id)
                .ToListAsync();
            reflection.EPAMappings = epaMappings;

            // Check if current user can edit (only EYD users can edit their own reflections)
            ViewBag.CanEdit = currentUser.Role == "EYD" && reflection.UserId == currentUser.Id;
            ViewBag.TargetUserId = reflection.UserId;

            return View(reflection);
        }

        // GET: Reflection/Create
        public async Task<IActionResult> Create(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = id ?? currentUser.Id;

            // Only EYD users can create reflections, and only for themselves
            if (currentUser.Role != "EYD" || targetUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Only EYD users can create reflections for themselves.";
                return RedirectToAction("Index", new { id = targetUserId });
            }

            ViewBag.TargetUserId = targetUserId;

            // Get available EPAs for selection
            ViewBag.EPAs = await _context.EPAs
                .OrderBy(epa => epa.Code)
                .ToListAsync();

            var viewModel = new CreateReflectionViewModel();
            return View(viewModel);
        }

        // POST: Reflection/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReflectionViewModel viewModel, string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = id ?? currentUser.Id;

            // Only EYD users can create reflections, and only for themselves
            if (currentUser.Role != "EYD" || targetUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Only EYD users can create reflections for themselves.";
                return RedirectToAction("Index", new { id = targetUserId });
            }

            if (ModelState.IsValid)
            {
                var reflection = new PortfolioReflection
                {
                    UserId = currentUser.Id,
                    Title = viewModel.Title,
                    WhenDidItHappen = viewModel.WhenDidItHappen,
                    ReasonsForWriting = viewModel.ReasonsForWriting,
                    NextSteps = viewModel.NextSteps,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsLocked = false
                };

                _context.Add(reflection);
                await _context.SaveChangesAsync();

                // Add EPA mappings
                if (viewModel.SelectedEPAIds != null && viewModel.SelectedEPAIds.Any())
                {
                    foreach (var epaId in viewModel.SelectedEPAIds)
                    {
                        var epaMapping = new EPAMapping
                        {
                            EPAId = epaId,
                            EntityType = "Reflection",
                            EntityId = reflection.Id,
                            UserId = currentUser.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.EPAMappings.Add(epaMapping);
                    }
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = "Reflection created successfully.";
                return RedirectToAction("Index", new { id = currentUser.Id });
            }

            ViewBag.TargetUserId = targetUserId;
            ViewBag.EPAs = await _context.EPAs
                .OrderBy(epa => epa.Code)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: Reflection/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get reflection without user restriction first
            var reflection = await _context.Reflections.FindAsync(id);
            if (reflection == null) return NotFound();

            // Only EYD users can edit reflections, and only their own
            if (currentUser.Role != "EYD" || reflection.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only edit your own reflections.";
                return RedirectToAction("Details", new { id = id });
            }

            // Check if reflection is locked
            if (reflection.IsLocked)
            {
                TempData["ErrorMessage"] = "This reflection is locked and cannot be edited.";
                return RedirectToAction("Details", new { id = id });
            }

            // Get current EPA mappings
            var currentEPAMappings = await _context.EPAMappings
                .Where(em => em.EntityType == "Reflection" && em.EntityId == reflection.Id)
                .Select(em => em.EPAId)
                .ToListAsync();

            ViewBag.TargetUserId = reflection.UserId;
            ViewBag.EPAs = await _context.EPAs
                .OrderBy(epa => epa.Code)
                .ToListAsync();

            var viewModel = new EditReflectionViewModel
            {
                Id = reflection.Id,
                Title = reflection.Title,
                WhenDidItHappen = reflection.WhenDidItHappen,
                ReasonsForWriting = reflection.ReasonsForWriting,
                NextSteps = reflection.NextSteps,
                SelectedEPAIds = currentEPAMappings,
                IsLocked = reflection.IsLocked,
                CreatedAt = reflection.CreatedAt,
                UpdatedAt = reflection.UpdatedAt
            };

            return View(viewModel);
        }

        // POST: Reflection/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditReflectionViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get the existing reflection
            var reflection = await _context.Reflections.FindAsync(id);
            if (reflection == null) return NotFound();

            // Only EYD users can edit reflections, and only their own
            if (currentUser.Role != "EYD" || reflection.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only edit your own reflections.";
                return RedirectToAction("Details", new { id = id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update reflection properties
                    reflection.Title = viewModel.Title;
                    reflection.WhenDidItHappen = viewModel.WhenDidItHappen;
                    reflection.ReasonsForWriting = viewModel.ReasonsForWriting;
                    reflection.NextSteps = viewModel.NextSteps;
                    reflection.IsLocked = viewModel.IsLocked;
                    reflection.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(reflection);

                    // Update EPA mappings
                    if (viewModel.SelectedEPAIds != null)
                    {
                        // Remove existing mappings
                        var existingMappings = _context.EPAMappings
                            .Where(em => em.EntityType == "Reflection" && em.EntityId == reflection.Id);
                        _context.EPAMappings.RemoveRange(existingMappings);

                        // Add new mappings
                        foreach (var epaId in viewModel.SelectedEPAIds)
                        {
                            var epaMapping = new EPAMapping
                            {
                                EPAId = epaId,
                                EntityType = "Reflection",
                                EntityId = reflection.Id,
                                UserId = currentUser.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.EPAMappings.Add(epaMapping);
                        }
                    }

                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Reflection updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReflectionExists(reflection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = reflection.Id });
            }

            ViewBag.TargetUserId = reflection.UserId;
            ViewBag.EPAs = await _context.EPAs
                .OrderBy(epa => epa.Code)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: Reflection/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get reflection without user restriction first
            var reflection = await _context.Reflections
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reflection == null) return NotFound();

            // Only EYD users can delete reflections, and only their own
            if (currentUser.Role != "EYD" || reflection.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only delete your own reflections.";
                return RedirectToAction("Details", new { id = id });
            }

            ViewBag.TargetUserId = reflection.UserId;
            return View(reflection);
        }

        // POST: Reflection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var reflection = await _context.Reflections.FindAsync(id);
            if (reflection == null) return NotFound();

            // Only EYD users can delete reflections, and only their own
            if (currentUser.Role != "EYD" || reflection.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only delete your own reflections.";
                return RedirectToAction("Index", new { id = reflection.UserId });
            }

            _context.Reflections.Remove(reflection);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Reflection deleted successfully.";
            return RedirectToAction("Index", new { id = currentUser.Id });
        }

        // POST: Reflection/ToggleLock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var reflection = await _context.Reflections.FindAsync(id);
            if (reflection == null) return NotFound();

            // Only EYD users can toggle lock on their own reflections
            if (currentUser.Role != "EYD" || reflection.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only lock/unlock your own reflections.";
                return RedirectToAction("Details", new { id = id });
            }

            reflection.IsLocked = !reflection.IsLocked;
            reflection.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(reflection);
            await _context.SaveChangesAsync();

            string status = reflection.IsLocked ? "locked" : "unlocked";
            TempData["SuccessMessage"] = $"Reflection has been {status}.";

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Reflection/GetAvailableEPAs
        [HttpGet]
        public async Task<IActionResult> GetAvailableEPAs()
        {
            try
            {
                var epas = await _context.EPAs
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Title)
                    .ToListAsync();
                var epaList = epas.Select(e => new
                {
                    id = e.Id,
                    code = e.Code,
                    title = e.Title,
                    description = e.Description
                }).ToList();

                return Json(new
                {
                    success = true,
                    epas = epaList,
                    minSelections = 2
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error loading EPAs: " + ex.Message
                });
            }
        }

        private bool ReflectionExists(int id)
        {
            return _context.Reflections.Any(e => e.Id == id);
        }
    }
}