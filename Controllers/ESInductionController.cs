using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class ESInductionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ESInductionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = id ?? currentUser.Id;

            // Authorization check for viewing ES Induction
            if (currentUser.Role == "EYD")
            {
                // EYD users can only view their own induction
                if (targetUserId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role == "ES")
            {
                // ES users can view inductions of their assigned EYD users
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

            // Get the ES induction for the target user
            var induction = await _context.ESInductions
                .Include(e => e.EYDUser)
                .Include(e => e.ESUser)
                .FirstOrDefaultAsync(e => e.EYDUserId == targetUserId);

            // Get the target user
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null) return NotFound();
            
            // Create the view model
            var viewModel = new ESInductionIndexViewModel
            {
                TargetUser = targetUser,
                ExistingInduction = induction
            };
            
            // Set ViewBag properties for UI control
            ViewBag.TargetUserName = targetUser.UserName ?? "Unknown User";
            ViewBag.TargetUserId = targetUserId;
            
            // Check if current user can edit (only ES users can edit)
            ViewBag.CanEdit = currentUser.Role == "ES" && 
                            (induction == null || induction.ESUserId == currentUser.Id);

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var induction = await _context.ESInductions
                .Include(e => e.EYDUser)
                .Include(e => e.ESUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (induction == null) return NotFound();

            // Check authorization based on user role
            if (currentUser.Role == "EYD")
            {
                // EYD users can only view their own induction
                if (induction.EYDUserId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role == "ES")
            {
                // ES users can view inductions of their assigned EYD users
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == induction.EYDUserId && 
                             assignment.IsActive);
                
                if (!isAssigned) return Forbid();
            }
            else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
            {
                return Forbid(); // Other roles are not allowed
            }

            // Set ViewBag properties for UI control
            ViewBag.CanEdit = currentUser.Role == "ES" && induction.ESUserId == currentUser.Id;
            ViewBag.TargetUserId = induction.EYDUserId;

            return View(induction);
        }

        public async Task<IActionResult> Create(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            // Only ES users can create ES Inductions
            if (currentUser.Role != "ES")
            {
                TempData["ErrorMessage"] = "Only Educational Supervisors can create induction records.";
                return RedirectToAction("Index", new { id = id });
            }

            // Must have a target user ID for ES users
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Target user ID is required.";
                return RedirectToAction("Index");
            }

            string targetUserId = id;
            
            // Security check for ES users - same pattern as LearningNeed controller
            var isAssigned = await _context.EYDESAssignments
                .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                         assignment.EYDUserId == targetUserId && 
                         assignment.IsActive);
            
            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "You can only create induction records for EYD users assigned to you.";
                return RedirectToAction("Index", new { id = targetUserId });
            }

            // Check if an induction already exists
            var existingInduction = await _context.ESInductions
                .FirstOrDefaultAsync(e => e.EYDUserId == targetUserId);
            
            if (existingInduction != null)
            {
                TempData["ErrorMessage"] = "An induction record already exists for this EYD user.";
                return RedirectToAction("Edit", new { id = existingInduction.Id });
            }

            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
            {
                TempData["ErrorMessage"] = "Target user not found.";
                return RedirectToAction("Index");
            }

            ViewBag.TargetUserId = targetUserId;
            ViewBag.TargetUserName = targetUser.UserName ?? "Unknown User";

            var viewModel = new CreateESInductionViewModel
            {
                EYDUserId = targetUserId,
                EYDUserName = targetUser.UserName ?? "Unknown User",
                MeetingDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("ESInduction/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateESInductionViewModel viewModel)
        {
            // IMMEDIATE DEBUG OUTPUT - FIRST LINE OF METHOD
            Console.WriteLine("████████ POST CREATE ACTION REACHED ████████");
            Console.WriteLine($"DateTime.Now: {DateTime.Now}");
            Console.WriteLine("=== POST Create Action Started ===");
            Console.WriteLine($"Received viewModel: EYDUserId={viewModel.EYDUserId}, MeetingDate={viewModel.MeetingDate}");
            Console.WriteLine($"HasReadTransitionDocumentAndAgreedPDP: {viewModel.HasReadTransitionDocumentAndAgreedPDP}");
            Console.WriteLine($"MeetingNotesAndComments: {viewModel.MeetingNotesAndComments}");
            Console.WriteLine($"PlacementDescription: {viewModel.PlacementDescription}");
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            string targetUserId = viewModel.EYDUserId;
            Console.WriteLine($"Target User ID: {targetUserId}");

            // Only ES users can create ES Inductions
            if (currentUser.Role != "ES")
            {
                Console.WriteLine($"User role check failed: {currentUser.Role}");
                TempData["ErrorMessage"] = "Only Educational Supervisors can create induction records.";
                return RedirectToAction("Index", new { id = targetUserId });
            }

            // ES users can only create inductions for their assigned EYD users
            var isAssigned = await _context.EYDESAssignments
                .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                         assignment.EYDUserId == targetUserId && 
                         assignment.IsActive);
            
            Console.WriteLine($"Assignment check: {isAssigned}");
            if (!isAssigned)
            {
                Console.WriteLine("Assignment check failed");
                TempData["ErrorMessage"] = "You can only create induction records for your assigned EYD users.";
                return RedirectToAction("Index", new { id = targetUserId });
            }

            // Debug: Log ModelState errors
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error - {modelError.Key}: {error.ErrorMessage}");
                    }
                }
            }

            // Debug: Log ModelState errors
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ModelState Validation Errors ===");
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error - {modelError.Key}: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid, creating induction...");
                try
                {
                    var induction = new ESInduction
                    {
                        EYDUserId = targetUserId,
                        ESUserId = currentUser.Id,
                        HasReadTransitionDocumentAndAgreedPDP = viewModel.HasReadTransitionDocumentAndAgreedPDP,
                        MeetingNotesAndComments = viewModel.MeetingNotesAndComments,
                        PlacementDescription = viewModel.PlacementDescription,
                        MeetingDate = DateTime.SpecifyKind(viewModel.MeetingDate ?? DateTime.Today, DateTimeKind.Utc),
                        IsCompleted = viewModel.IsCompleted,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    Console.WriteLine($"About to save ESInduction with HasReadTransitionDocumentAndAgreedPDP = {induction.HasReadTransitionDocumentAndAgreedPDP}");

                    if (viewModel.IsCompleted)
                    {
                        induction.CompletedAt = DateTime.UtcNow;
                    }

                    Console.WriteLine("Adding induction to context...");
                    _context.Add(induction);
                    
                    Console.WriteLine("Saving changes...");
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine("Induction saved successfully!");
                    TempData["SuccessMessage"] = "ES Induction record created successfully.";
                    return RedirectToAction("Index", new { id = targetUserId });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception during save: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                    throw;
                }
            }

            // ModelState validation failed, repopulate the viewModel
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            ViewBag.TargetUserId = targetUserId;
            ViewBag.TargetUserName = targetUser?.UserName ?? "Unknown User";
            
            // Repopulate the viewModel properties that aren't preserved during model binding
            viewModel.EYDUserId = targetUserId;
            viewModel.EYDUserName = targetUser?.UserName ?? "Unknown User";

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var induction = await _context.ESInductions
                .Include(e => e.EYDUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (induction == null) return NotFound();

            // Only ES users can edit, and only their own induction records
            if (currentUser.Role != "ES" || induction.ESUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only edit your own induction records.";
                return RedirectToAction("Index", new { id = induction.EYDUserId });
            }

            ViewBag.TargetUserId = induction.EYDUserId;
            ViewBag.TargetUserName = induction.EYDUser?.UserName ?? "Unknown User";

            var viewModel = new EditESInductionViewModel
            {
                Id = induction.Id,
                EYDUserId = induction.EYDUserId,
                EYDUserName = induction.EYDUser?.UserName ?? "Unknown User",
                HasReadTransitionDocumentAndAgreedPDP = induction.HasReadTransitionDocumentAndAgreedPDP,
                MeetingNotesAndComments = induction.MeetingNotesAndComments,
                PlacementDescription = induction.PlacementDescription,
                MeetingDate = induction.MeetingDate,
                IsCompleted = induction.IsCompleted
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditESInductionViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var induction = await _context.ESInductions.FindAsync(id);
            if (induction == null) return NotFound();

            // Only ES users can edit, and only their own induction records
            if (currentUser.Role != "ES" || induction.ESUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only edit your own induction records.";
                return RedirectToAction("Index", new { id = induction.EYDUserId });
            }

            if (ModelState.IsValid)
            {
                induction.HasReadTransitionDocumentAndAgreedPDP = viewModel.HasReadTransitionDocumentAndAgreedPDP;
                induction.MeetingNotesAndComments = viewModel.MeetingNotesAndComments;
                induction.PlacementDescription = viewModel.PlacementDescription;
                induction.MeetingDate = DateTime.SpecifyKind(viewModel.MeetingDate ?? DateTime.Today, DateTimeKind.Utc);
                induction.UpdatedAt = DateTime.UtcNow;

                // Handle completion status
                if (viewModel.IsCompleted && !induction.IsCompleted)
                {
                    induction.IsCompleted = true;
                    induction.CompletedAt = DateTime.UtcNow;
                }
                else if (!viewModel.IsCompleted && induction.IsCompleted)
                {
                    induction.IsCompleted = false;
                    induction.CompletedAt = null;
                }

                _context.Update(induction);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "ES Induction record updated successfully.";
                return RedirectToAction("Index", new { id = induction.EYDUserId });
            }

            var targetUser = await _userManager.FindByIdAsync(induction.EYDUserId);
            ViewBag.TargetUserId = induction.EYDUserId;
            ViewBag.TargetUserName = targetUser?.UserName ?? "Unknown User";

            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var induction = await _context.ESInductions
                .Include(e => e.EYDUser)
                .Include(e => e.ESUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (induction == null) return NotFound();

            // Only ES users can delete, and only their own induction records
            if (currentUser.Role != "ES" || induction.ESUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only delete your own induction records.";
                return RedirectToAction("Index", new { id = induction.EYDUserId });
            }

            ViewBag.TargetUserId = induction.EYDUserId;
            return View(induction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var induction = await _context.ESInductions.FindAsync(id);
            if (induction == null) return NotFound();

            // Only ES users can delete, and only their own induction records
            if (currentUser.Role != "ES" || induction.ESUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only delete your own induction records.";
                return RedirectToAction("Index", new { id = induction.EYDUserId });
            }

            string eydUserId = induction.EYDUserId;
            _context.ESInductions.Remove(induction);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "ES Induction record deleted successfully.";
            return RedirectToAction("Index", new { id = eydUserId });
        }
    }
}
