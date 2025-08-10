using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.Services;
using EYDGateway.ViewModels;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class ProtectedLearningTimeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEPAService _epaService;

        public ProtectedLearningTimeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEPAService epaService)
        {
            _context = context;
            _userManager = userManager;
            _epaService = epaService;
        }

        // GET: ProtectedLearningTime
        public async Task<IActionResult> Index(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = id ?? currentUser.Id;

            // Authorization check for viewing PLT entries
            if (currentUser.Role == "EYD")
            {
                // EYD users can only view their own PLT entries
                if (targetUserId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role == "ES")
            {
                // ES users can view PLT entries of their assigned EYD users
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == targetUserId && 
                             assignment.IsActive);
                
                if (!isAssigned) return Forbid();
            }
            else if (currentUser.Role == "TPD" || currentUser.Role == "Dean")
            {
                // TPD and Dean users can view PLT entries for any EYD user
                // No additional authorization check needed
            }
            else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
            {
                return Forbid(); // Other roles are not allowed
            }

            var pltEntries = await _context.ProtectedLearningTimes
                .Where(p => p.UserId == targetUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Explicitly load EPA mappings for each PLT entry
            foreach (var plt in pltEntries)
            {
                var epaMappings = await _context.EPAMappings
                    .Include(em => em.EPA)
                    .Where(em => em.EntityType == "ProtectedLearningTime" && em.EntityId == plt.Id)
                    .ToListAsync();
                plt.EPAMappings = epaMappings;
            }

            // Get target user info for display
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            ViewBag.TargetUserName = targetUser?.UserName ?? "Unknown User";
            ViewBag.TargetUserId = targetUserId;
            
            // Check if current user can edit (only EYD users can edit their own PLT entries)
            ViewBag.CanEdit = currentUser.Role == "EYD" && targetUserId == currentUser.Id;

            return View(pltEntries);
        }

        // GET: ProtectedLearningTime/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get PLT entry without user restriction first
            var plt = await _context.ProtectedLearningTimes
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plt == null) return NotFound();

            // Check authorization based on user role
            if (currentUser.Role == "EYD")
            {
                // EYD users can only view their own PLT entries
                if (plt.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role == "ES")
            {
                // ES users can view PLT entries of their assigned EYD users
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == plt.UserId && 
                             assignment.IsActive);
                
                if (!isAssigned) return Forbid();
            }
            else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
            {
                return Forbid(); // Other roles are not allowed
            }

            // Explicitly load EPA mappings for this PLT entry
            var epaMappings = await _context.EPAMappings
                .Include(em => em.EPA)
                .Where(em => em.EntityType == "ProtectedLearningTime" && em.EntityId == plt.Id)
                .ToListAsync();
            plt.EPAMappings = epaMappings;

            // Check if current user can edit (only EYD users can edit their own PLT entries)
            ViewBag.CanEdit = currentUser.Role == "EYD" && plt.UserId == currentUser.Id;
            ViewBag.TargetUserId = plt.UserId;

            return View(plt);
        }

        // GET: ProtectedLearningTime/Create
        public async Task<IActionResult> Create(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = id ?? currentUser.Id;

            // Only EYD users can create PLT entries, and only for themselves
            if (currentUser.Role != "EYD" || targetUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Only EYD users can create Protected Learning Time entries for themselves.";
                return RedirectToAction("Index", new { id = targetUserId });
            }

            var viewModel = new CreateProtectedLearningTimeViewModel();
            ViewBag.TargetUserId = targetUserId;
            return View(viewModel);
        }

        // POST: ProtectedLearningTime/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProtectedLearningTimeViewModel model, string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = id ?? currentUser.Id;

            // Only EYD users can create PLT entries, and only for themselves
            if (currentUser.Role != "EYD" || targetUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Only EYD users can create Protected Learning Time entries for themselves.";
                return RedirectToAction("Index", new { id = targetUserId });
            }

            // Debug: Log received EPA IDs
            Console.WriteLine($"DEBUG PLT: Received SelectedEPAIds: [{string.Join(", ", model.SelectedEPAIds)}]");
            Console.WriteLine($"DEBUG PLT: SelectedEPAIds count: {model.SelectedEPAIds.Count}");

            // Validate EPA selection (minimum 2 required)
            if (model.SelectedEPAIds.Count < 2)
            {
                ModelState.AddModelError("SelectedEPAIds", "Please select at least 2 EPAs for this Protected Learning Time.");
            }

            if (ModelState.IsValid)
            {
                var plt = new ProtectedLearningTime
                {
                    Title = model.Title,
                    Format = model.Format,
                    LengthOfPLT = model.LengthOfPLT,
                    WhenAndWhoLed = model.WhenAndWhoLed,
                    BriefOutlineOfLearning = model.BriefOutlineOfLearning,
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsLocked = false
                };

                _context.ProtectedLearningTimes.Add(plt);
                await _context.SaveChangesAsync();

                // Create EPA mappings
                Console.WriteLine($"DEBUG PLT: Creating EPA mappings for {model.SelectedEPAIds.Count} EPAs");
                foreach (var epaId in model.SelectedEPAIds)
                {
                    Console.WriteLine($"DEBUG PLT: Creating mapping for EPA ID: {epaId}");
                    var mapping = new EPAMapping
                    {
                        EPAId = epaId,
                        EntityType = "ProtectedLearningTime",
                        EntityId = plt.Id,
                        UserId = currentUser.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.EPAMappings.Add(mapping);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG PLT: Successfully saved PLT with {model.SelectedEPAIds.Count} EPA mappings");
                TempData["SuccessMessage"] = "Protected Learning Time created successfully.";
                return RedirectToAction("Index", new { id = currentUser.Id });
            }

            ViewBag.TargetUserId = targetUserId;
            return View(model);
        }

        // GET: ProtectedLearningTime/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get PLT entry without user restriction first
            var plt = await _context.ProtectedLearningTimes.FindAsync(id);
            if (plt == null) return NotFound();

            // Only EYD users can edit PLT entries, and only their own
            if (currentUser.Role != "EYD" || plt.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only edit your own Protected Learning Time entries.";
                return RedirectToAction("Details", new { id = id });
            }

            if (plt.IsLocked)
            {
                TempData["ErrorMessage"] = "This Protected Learning Time entry is locked and cannot be edited.";
                return RedirectToAction("Details", new { id = id });
            }

            // Load existing EPA mappings
            var existingEPAMappings = await _context.EPAMappings
                .Where(em => em.EntityType == "ProtectedLearningTime" && em.EntityId == plt.Id)
                .ToListAsync();

            var viewModel = new EditProtectedLearningTimeViewModel
            {
                Id = plt.Id,
                Title = plt.Title,
                Format = plt.Format,
                LengthOfPLT = plt.LengthOfPLT,
                WhenAndWhoLed = plt.WhenAndWhoLed,
                BriefOutlineOfLearning = plt.BriefOutlineOfLearning,
                IsLocked = plt.IsLocked,
                SelectedEPAIds = existingEPAMappings.Select(em => em.EPAId).ToList()
            };

            ViewBag.TargetUserId = plt.UserId;
            return View(viewModel);
        }

        // POST: ProtectedLearningTime/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProtectedLearningTimeViewModel model)
        {
            if (id != model.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get PLT entry to check ownership
            var plt = await _context.ProtectedLearningTimes.FindAsync(id);
            if (plt == null) return NotFound();

            // Only EYD users can edit PLT entries, and only their own
            if (currentUser.Role != "EYD" || plt.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only edit your own Protected Learning Time entries.";
                return RedirectToAction("Details", new { id = id });
            }

            // Validate EPA selection (minimum 2 required)
            if (model.SelectedEPAIds.Count < 2)
            {
                ModelState.AddModelError("SelectedEPAIds", "Please select at least 2 EPAs for this Protected Learning Time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (plt.IsLocked && !model.IsLocked)
                    {
                        TempData["ErrorMessage"] = "Cannot unlock a Protected Learning Time entry.";
                        return RedirectToAction("Details", new { id = id });
                    }

                    plt.Title = model.Title;
                    plt.Format = model.Format;
                    plt.LengthOfPLT = model.LengthOfPLT;
                    plt.WhenAndWhoLed = model.WhenAndWhoLed;
                    plt.BriefOutlineOfLearning = model.BriefOutlineOfLearning;
                    plt.IsLocked = model.IsLocked;
                    plt.UpdatedAt = DateTime.UtcNow;

                    _context.Update(plt);

                    // Remove existing EPA mappings
                    var existingMappings = await _context.EPAMappings
                        .Where(em => em.EntityType == "ProtectedLearningTime" && em.EntityId == plt.Id)
                        .ToListAsync();
                    _context.EPAMappings.RemoveRange(existingMappings);

                    // Add new EPA mappings
                    foreach (var epaId in model.SelectedEPAIds)
                    {
                        var mapping = new EPAMapping
                        {
                            EPAId = epaId,
                            EntityType = "ProtectedLearningTime",
                            EntityId = plt.Id,
                            UserId = currentUser.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.EPAMappings.Add(mapping);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Protected Learning Time updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = plt.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProtectedLearningTimeExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        // GET: ProtectedLearningTime/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Get PLT entry without user restriction first
            var plt = await _context.ProtectedLearningTimes
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plt == null) return NotFound();

            // Only EYD users can delete PLT entries, and only their own
            if (currentUser.Role != "EYD" || plt.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only delete your own Protected Learning Time entries.";
                return RedirectToAction("Details", new { id = id });
            }

            // Load EPA mappings for display
            var epaMappings = await _context.EPAMappings
                .Include(em => em.EPA)
                .Where(em => em.EntityType == "ProtectedLearningTime" && em.EntityId == plt.Id)
                .ToListAsync();
            plt.EPAMappings = epaMappings;

            ViewBag.TargetUserId = plt.UserId;
            return View(plt);
        }

        // POST: ProtectedLearningTime/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var plt = await _context.ProtectedLearningTimes.FindAsync(id);
            if (plt == null) return NotFound();

            // Only EYD users can delete PLT entries, and only their own
            if (currentUser.Role != "EYD" || plt.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only delete your own Protected Learning Time entries.";
                return RedirectToAction("Index", new { id = plt.UserId });
            }

            // Remove EPA mappings first
            var epaMappings = await _context.EPAMappings
                .Where(em => em.EntityType == "ProtectedLearningTime" && em.EntityId == plt.Id)
                .ToListAsync();
            _context.EPAMappings.RemoveRange(epaMappings);

            _context.ProtectedLearningTimes.Remove(plt);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Protected Learning Time deleted successfully.";
            return RedirectToAction("Index", new { id = currentUser.Id });
        }

        // AJAX endpoint to get available EPAs
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
                    message = ex.Message
                });
            }
        }

        private bool ProtectedLearningTimeExists(int id)
        {
            return _context.ProtectedLearningTimes.Any(e => e.Id == id);
        }
    }
}
