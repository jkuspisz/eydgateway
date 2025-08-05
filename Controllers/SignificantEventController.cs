using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class SignificantEventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SignificantEventController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: SignificantEvent
        public async Task<IActionResult> Index(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId;
            bool canEdit = false;

            // Determine target user and permissions
            if (!string.IsNullOrEmpty(id))
            {
                // Viewing someone else's significant events
                targetUserId = id;
                
                // Check permissions
                if (currentUser.Id == targetUserId)
                {
                    canEdit = true;
                }
                else if (currentUser.Role == "ES")
                {
                    // ES can view assigned EYD users' significant events (view-only)
                    var isAssigned = await _context.EYDESAssignments
                        .AnyAsync(a => a.ESUserId == currentUser.Id && 
                                      a.EYDUserId == targetUserId && 
                                      a.IsActive);
                    if (!isAssigned) return Forbid();
                    canEdit = false; // ES cannot edit
                }
                else if (currentUser.Role == "Admin" || currentUser.Role == "Superuser")
                {
                    canEdit = false; // Admin can view but not edit
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                // Viewing own significant events
                targetUserId = currentUser.Id;
                canEdit = true;
            }

            var targetUser = await _context.Users.FindAsync(targetUserId);
            if (targetUser == null) return NotFound();

            var significantEvents = await _context.SignificantEvents
                .Where(se => se.UserId == targetUserId)
                .OrderByDescending(se => se.CreatedAt)
                .ToListAsync();

            // Build view models with EPA data
            var viewModels = new List<SignificantEventIndexViewModel>();
            
            foreach (var se in significantEvents)
            {
                var epaMappings = await _context.EPAMappings
                    .Where(em => em.EntityType == "SignificantEvent" && em.EntityId == se.Id)
                    .Include(em => em.EPA)
                    .ToListAsync();

                viewModels.Add(new SignificantEventIndexViewModel
                {
                    Id = se.Id,
                    Title = se.Title,
                    IsLocked = se.IsLocked,
                    ESSignedOff = se.ESSignedOff,
                    TPDSignedOff = se.TPDSignedOff,
                    CreatedAt = se.CreatedAt,
                    UpdatedAt = se.UpdatedAt,
                    EPACount = epaMappings.Count,
                    EPAs = epaMappings.Select(em => new EPAMappingViewModel
                    {
                        Id = em.Id,
                        EPAId = em.EPAId,
                        EPACode = em.EPA.Code,
                        EPATitle = em.EPA.Title,
                        EPADescription = em.EPA.Description
                    }).ToList()
                });
            }

            ViewBag.CanEdit = canEdit;
            ViewBag.TargetUser = targetUser;
            ViewBag.IsViewingOthersContent = currentUser.Id != targetUserId;

            return View(viewModels);
        }

        // GET: SignificantEvent/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var significantEvent = await _context.SignificantEvents
                .Include(se => se.User)
                .Include(se => se.ESUser)
                .Include(se => se.TPDUser)
                .FirstOrDefaultAsync(se => se.Id == id);

            if (significantEvent == null) return NotFound();

            // Load EPA mappings separately
            var epaMappings = await _context.EPAMappings
                .Where(em => em.EntityType == "SignificantEvent" && em.EntityId == significantEvent.Id)
                .Include(em => em.EPA)
                .ToListAsync();

            if (significantEvent == null) return NotFound();

            // Check permissions
            bool canAccess = false;
            if (significantEvent.UserId == currentUser.Id)
            {
                canAccess = true;
            }
            else if (currentUser.Role == "ES")
            {
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(a => a.ESUserId == currentUser.Id && 
                                  a.EYDUserId == significantEvent.UserId && 
                                  a.IsActive);
                canAccess = isAssigned;
            }
            else if (currentUser.Role == "Admin" || currentUser.Role == "Superuser")
            {
                canAccess = true;
            }

            if (!canAccess) return Forbid();

            var viewModel = new SignificantEventDetailsViewModel
            {
                Id = significantEvent.Id,
                Title = significantEvent.Title,
                AccountOfExperience = significantEvent.AccountOfExperience,
                AnalysisOfSituation = significantEvent.AnalysisOfSituation,
                ReflectionOnEvent = significantEvent.ReflectionOnEvent,
                IsLocked = significantEvent.IsLocked,
                ESSignedOff = significantEvent.ESSignedOff,
                TPDSignedOff = significantEvent.TPDSignedOff,
                ESSignedOffAt = significantEvent.ESSignedOffAt,
                TPDSignedOffAt = significantEvent.TPDSignedOffAt,
                ESUserName = significantEvent.ESUser?.DisplayName,
                TPDUserName = significantEvent.TPDUser?.DisplayName,
                CreatedAt = significantEvent.CreatedAt,
                UpdatedAt = significantEvent.UpdatedAt,
                UserName = significantEvent.User.DisplayName,
                UserId = significantEvent.UserId,
                EPAMappings = epaMappings.Select(em => new EPAMappingViewModel
                {
                    Id = em.Id,
                    EPAId = em.EPAId,
                    EPACode = em.EPA.Code,
                    EPATitle = em.EPA.Title,
                    EPADescription = em.EPA.Description
                }).ToList()
            };

            ViewBag.CanEdit = significantEvent.UserId == currentUser.Id && !significantEvent.IsLocked;
            ViewBag.CanSignOff = CanUserSignOff(currentUser, significantEvent);

            return View(viewModel);
        }

        // GET: SignificantEvent/Create
        public IActionResult Create()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser?.Role != "EYD") return Forbid();

            return View(new CreateSignificantEventViewModel());
        }

        // POST: SignificantEvent/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSignificantEventViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "EYD") return Forbid();

            if (ModelState.IsValid)
            {
                var significantEvent = new SignificantEvent
                {
                    UserId = currentUser.Id,
                    Title = model.Title,
                    AccountOfExperience = model.AccountOfExperience,
                    AnalysisOfSituation = model.AnalysisOfSituation,
                    ReflectionOnEvent = model.ReflectionOnEvent,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SignificantEvents.Add(significantEvent);
                await _context.SaveChangesAsync();

                // Add EPA mappings
                if (model.SelectedEPAIds?.Any() == true)
                {
                    foreach (var epaId in model.SelectedEPAIds)
                    {
                        var epaMapping = new EPAMapping
                        {
                            EPAId = epaId,
                            EntityType = "SignificantEvent",
                            EntityId = significantEvent.Id,
                            UserId = currentUser.Id
                        };
                        _context.EPAMappings.Add(epaMapping);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Significant Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: SignificantEvent/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var significantEvent = await _context.SignificantEvents
                .Include(se => se.ESUser)
                .Include(se => se.TPDUser)
                .FirstOrDefaultAsync(se => se.Id == id);

            if (significantEvent == null) return NotFound();

            // Check permissions
            if (significantEvent.UserId != currentUser.Id) return Forbid();
            if (significantEvent.IsLocked) return Forbid();

            // Get current EPA mappings
            var currentEPAMappings = await _context.EPAMappings
                .Where(em => em.EntityType == "SignificantEvent" && em.EntityId == significantEvent.Id)
                .Select(em => em.EPAId)
                .ToListAsync();

            var viewModel = new EditSignificantEventViewModel
            {
                Id = significantEvent.Id,
                Title = significantEvent.Title,
                AccountOfExperience = significantEvent.AccountOfExperience,
                AnalysisOfSituation = significantEvent.AnalysisOfSituation,
                ReflectionOnEvent = significantEvent.ReflectionOnEvent,
                IsLocked = significantEvent.IsLocked,
                ESSignedOff = significantEvent.ESSignedOff,
                TPDSignedOff = significantEvent.TPDSignedOff,
                ESSignedOffAt = significantEvent.ESSignedOffAt,
                TPDSignedOffAt = significantEvent.TPDSignedOffAt,
                ESUserName = significantEvent.ESUser?.DisplayName,
                TPDUserName = significantEvent.TPDUser?.DisplayName,
                SelectedEPAIds = currentEPAMappings
            };

            return View(viewModel);
        }

        // POST: SignificantEvent/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditSignificantEventViewModel model)
        {
            if (id != model.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var significantEvent = await _context.SignificantEvents
                .FirstOrDefaultAsync(se => se.Id == id);

            if (significantEvent == null) return NotFound();

            // Check permissions
            if (significantEvent.UserId != currentUser.Id) return Forbid();
            if (significantEvent.IsLocked) return Forbid();

            if (ModelState.IsValid)
            {
                significantEvent.Title = model.Title;
                significantEvent.AccountOfExperience = model.AccountOfExperience;
                significantEvent.AnalysisOfSituation = model.AnalysisOfSituation;
                significantEvent.ReflectionOnEvent = model.ReflectionOnEvent;
                significantEvent.UpdatedAt = DateTime.UtcNow;

                // Update EPA mappings
                var existingMappings = await _context.EPAMappings
                    .Where(em => em.EntityType == "SignificantEvent" && em.EntityId == significantEvent.Id)
                    .ToListAsync();
                _context.EPAMappings.RemoveRange(existingMappings);

                if (model.SelectedEPAIds?.Any() == true)
                {
                    foreach (var epaId in model.SelectedEPAIds)
                    {
                        var epaMapping = new EPAMapping
                        {
                            EPAId = epaId,
                            EntityType = "SignificantEvent",
                            EntityId = significantEvent.Id,
                            UserId = currentUser.Id
                        };
                        _context.EPAMappings.Add(epaMapping);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Significant Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: SignificantEvent/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var significantEvent = await _context.SignificantEvents
                .Include(se => se.User)
                .FirstOrDefaultAsync(se => se.Id == id);

            if (significantEvent == null) return NotFound();

            // Check permissions
            if (significantEvent.UserId != currentUser.Id) return Forbid();
            if (significantEvent.IsLocked) return Forbid();

            var viewModel = new SignificantEventDetailsViewModel
            {
                Id = significantEvent.Id,
                Title = significantEvent.Title,
                AccountOfExperience = significantEvent.AccountOfExperience,
                AnalysisOfSituation = significantEvent.AnalysisOfSituation,
                ReflectionOnEvent = significantEvent.ReflectionOnEvent,
                IsLocked = significantEvent.IsLocked,
                CreatedAt = significantEvent.CreatedAt,
                UpdatedAt = significantEvent.UpdatedAt,
                UserName = significantEvent.User.DisplayName
            };

            return View(viewModel);
        }

        // POST: SignificantEvent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var significantEvent = await _context.SignificantEvents
                .FirstOrDefaultAsync(se => se.Id == id);

            if (significantEvent == null) return NotFound();

            // Check permissions
            if (significantEvent.UserId != currentUser.Id) return Forbid();
            if (significantEvent.IsLocked) return Forbid();

            // Remove EPA mappings first
            var epaMappings = await _context.EPAMappings
                .Where(em => em.EntityType == "SignificantEvent" && em.EntityId == significantEvent.Id)
                .ToListAsync();
            _context.EPAMappings.RemoveRange(epaMappings);
            _context.SignificantEvents.Remove(significantEvent);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Significant Event deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: SignificantEvent/ESSignOff/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ESSignOff(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "ES") return Forbid();

            var significantEvent = await _context.SignificantEvents.FindAsync(id);
            if (significantEvent == null) return NotFound();

            // Check if ES is assigned to this EYD
            var isAssigned = await _context.EYDESAssignments
                .AnyAsync(a => a.ESUserId == currentUser.Id && 
                              a.EYDUserId == significantEvent.UserId && 
                              a.IsActive);
            if (!isAssigned) return Forbid();

            significantEvent.ESSignedOff = true;
            significantEvent.ESSignedOffAt = DateTime.UtcNow;
            significantEvent.ESUserId = currentUser.Id;
            significantEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Significant Event signed off by ES successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: SignificantEvent/TPDSignOff/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TPDSignOff(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Role != "TPD") return Forbid();

            var significantEvent = await _context.SignificantEvents
                .Include(se => se.User)
                    .ThenInclude(u => u.Scheme)
                        .ThenInclude(s => s.Area)
                .FirstOrDefaultAsync(se => se.Id == id);

            if (significantEvent == null) return NotFound();

            // Check if TPD is assigned to the same area as the EYD
            if (significantEvent.User.Scheme?.Area?.Id != currentUser.AreaId) return Forbid();

            significantEvent.TPDSignedOff = true;
            significantEvent.TPDSignedOffAt = DateTime.UtcNow;
            significantEvent.TPDUserId = currentUser.Id;
            significantEvent.UpdatedAt = DateTime.UtcNow;

            // Lock the significant event after TPD sign-off
            significantEvent.IsLocked = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Significant Event signed off by TPD successfully! The event is now locked.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // AJAX endpoint for EPA loading
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

        private static bool CanUserSignOff(ApplicationUser user, SignificantEvent significantEvent)
        {
            return user.Role switch
            {
                "ES" => !significantEvent.ESSignedOff,
                "TPD" => significantEvent.ESSignedOff && !significantEvent.TPDSignedOff,
                _ => false
            };
        }
    }
}
