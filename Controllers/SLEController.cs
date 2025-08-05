using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;
using EYDGateway.Services;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class SLEController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEPAService _epaService;

        public SLEController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEPAService epaService)
        {
            _context = context;
            _userManager = userManager;
            _epaService = epaService;
        }

        public async Task<IActionResult> Index(string? id = null, string? type = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Determine target user ID
            string targetUserId;
            if (string.IsNullOrEmpty(id))
            {
                targetUserId = currentUser.Id;
            }
            else
            {
                targetUserId = id;
                
                // Security check for ES users viewing EYD data
                if (currentUser.Role == "ES")
                {
                    var isAssigned = await _context.EYDESAssignments
                        .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                                 assignment.EYDUserId == targetUserId && 
                                 assignment.IsActive);
                    
                    if (!isAssigned)
                    {
                        return Forbid("You can only view SLE data for EYD users assigned to you.");
                    }
                }
                else if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                {
                    return Forbid("You can only view your own SLE data.");
                }
            }

            // Get SLEs for the target user (or where current user is assessor)
            var query = _context.SLEs
                .Include(s => s.EYDUser)
                .Include(s => s.AssessorUser)
                .Include(s => s.EPAMappings)
                    .ThenInclude(em => em.EPA)
                .Where(s => s.EYDUserId == targetUserId || s.AssessorUserId == currentUser.Id);

            // Apply type filter if specified
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(s => s.SLEType == type);
            }

            var sles = await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var viewModel = new SLEListViewModel
            {
                SLEs = sles.Select(s => new SLESummaryItem
                {
                    Id = s.Id,
                    Title = s.Title,
                    SLEType = s.SLEType,
                    ScheduledDate = s.ScheduledDate,
                    AssessorName = !string.IsNullOrEmpty(s.AssessorUserId) 
                        ? s.AssessorUser?.DisplayName ?? "Unknown" 
                        : s.ExternalAssessorName ?? "External",
                    IsInternalAssessor = s.IsInternalAssessor,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    IsAssessmentCompleted = s.IsAssessmentCompleted,
                    LinkedEPAs = s.EPAMappings?.Select(em => em.EPA?.Code ?? "").ToList() ?? new List<string>()
                }).ToList(),
                CanCreateSLE = await CanUserCreateSLE(currentUser)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? sleType = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await CanUserCreateSLE(user))
            {
                TempData["Error"] = "You don't have permission to create SLEs.";
                return RedirectToAction("Index", "EYD");
            }

            // Get the EYD's assigned ES
            var assignedES = await _context.EYDESAssignments
                .Include(a => a.ESUser)
                .Where(a => a.EYDUserId == user.Id && a.IsActive)
                .Select(a => a.ESUser)
                .FirstOrDefaultAsync();

            var viewModel = new CreateSLEViewModel
            {
                SLEType = sleType ?? string.Empty,
                AvailableSLETypes = GetAvailableSLETypes(),
                AvailableAssessors = await GetAvailableAssessorsInArea(user),
                AssignedES = assignedES
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSLEViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Debug: Log received EPA IDs
            Console.WriteLine($"DEBUG: Received SelectedEPAIds: [{string.Join(", ", model.SelectedEPAIds)}]");
            Console.WriteLine($"DEBUG: SelectedEPAIds count: {model.SelectedEPAIds.Count}");

            // Debug: Log user role
            var isEYD = await _userManager.IsInRoleAsync(user, "EYD");
            var isSuperuser = await _userManager.IsInRoleAsync(user, "Superuser");
            Console.WriteLine($"DEBUG: User {user.Email} - IsEYD: {isEYD}, IsSuperuser: {isSuperuser}");

            if (!await CanUserCreateSLE(user))
            {
                TempData["Error"] = "You don't have permission to create SLEs.";
                return RedirectToAction("Index", "EYD");
            }

            // Debug: Log EPA validation
            var isEPASelectionValid = await _epaService.ValidateSLEEPASelectionAsync(
                model.SLEType, model.SelectedEPAIds);
            Console.WriteLine($"DEBUG: EPA validation - SLEType: {model.SLEType}, SelectedEPAs: {string.Join(",", model.SelectedEPAIds)}, Valid: {isEPASelectionValid}");
            Console.WriteLine($"DEBUG: EPA validation details - Required: true, Received count: {model.SelectedEPAIds.Count}");
            
            if (!isEPASelectionValid)
            {
                var singleEPATypes = new[] { "MiniCEX", "DOPS", "DOPSSim" };
                var errorMessage = singleEPATypes.Contains(model.SLEType) 
                    ? "This SLE type requires exactly 1 EPA selection."
                    : "This SLE type requires 1-2 EPA selections.";
                ModelState.AddModelError("SelectedEPAIds", errorMessage);
                Console.WriteLine($"DEBUG: EPA validation failed - {errorMessage}");
            }

            // Validate assessor selection
            Console.WriteLine($"DEBUG: Assessor validation - IsInternal: {model.IsInternalAssessor}, AssessorUserId: {model.AssessorUserId}, ExternalName: {model.ExternalAssessorName}");
            if (model.IsInternalAssessor && string.IsNullOrEmpty(model.AssessorUserId))
            {
                ModelState.AddModelError("AssessorUserId", "Please select an internal assessor.");
                Console.WriteLine("DEBUG: Internal assessor validation failed - no assessor selected");
            }
            else if (!model.IsInternalAssessor)
            {
                if (string.IsNullOrEmpty(model.ExternalAssessorName))
                {
                    ModelState.AddModelError("ExternalAssessorName", "External assessor name is required.");
                    Console.WriteLine("DEBUG: External assessor validation failed - no name");
                }
                if (string.IsNullOrEmpty(model.ExternalAssessorEmail))
                {
                    ModelState.AddModelError("ExternalAssessorEmail", "External assessor email is required.");
                    Console.WriteLine("DEBUG: External assessor validation failed - no email");
                }
            }

            // Validate location fields based on SLE type
            Console.WriteLine($"DEBUG: Location validation - SLEType: {model.SLEType}");
            switch (model.SLEType)
            {
                case "CBD":
                case "DOPS":
                case "DOPSSim":
                case "MiniCEX":
                    if (string.IsNullOrEmpty(model.Location))
                    {
                        ModelState.AddModelError("Location", "Location is required for this SLE type.");
                        Console.WriteLine("DEBUG: Location validation failed - Location required but empty");
                    }
                    // Clear other fields that shouldn't be validated
                    ModelState.Remove("Setting");
                    ModelState.Remove("Audience");
                    ModelState.Remove("AudienceSetting");
                    break;
                case "DENTL":
                    if (string.IsNullOrEmpty(model.Setting))
                    {
                        ModelState.AddModelError("Setting", "Setting is required for DENTL.");
                        Console.WriteLine("DEBUG: Setting validation failed - Setting required but empty");
                    }
                    // Clear other fields that shouldn't be validated
                    ModelState.Remove("Location");
                    ModelState.Remove("Audience");
                    ModelState.Remove("AudienceSetting");
                    break;
                case "DtCT":
                    if (string.IsNullOrEmpty(model.Audience))
                    {
                        ModelState.AddModelError("Audience", "Audience is required for DtCT.");
                        Console.WriteLine("DEBUG: Audience validation failed - Audience required but empty");
                    }
                    if (string.IsNullOrEmpty(model.AudienceSetting))
                    {
                        ModelState.AddModelError("AudienceSetting", "Audience Setting is required for DtCT.");
                        Console.WriteLine("DEBUG: AudienceSetting validation failed - AudienceSetting required but empty");
                    }
                    // Clear other fields that shouldn't be validated
                    ModelState.Remove("Location");
                    ModelState.Remove("Setting");
                    break;
                default:
                    // Clear all location fields for unknown types
                    ModelState.Remove("Location");
                    ModelState.Remove("Setting");
                    ModelState.Remove("Audience");
                    ModelState.Remove("AudienceSetting");
                    break;
            }

            Console.WriteLine($"DEBUG: ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: ModelState validation failed. Errors:");
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"DEBUG: Field '{error.Key}' has errors: {string.Join("; ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                model.AvailableSLETypes = GetAvailableSLETypes();
                model.AvailableAssessors = await GetAvailableAssessorsInArea(user);
                Console.WriteLine($"DEBUG: Returning view with model. SelectedEPAIds still contains: [{string.Join(", ", model.SelectedEPAIds)}]");
                return View(model);
            }

            var sle = new SLE
            {
                SLEType = model.SLEType,
                Title = model.Title,
                Description = model.Description,
                ScheduledDate = DateTime.SpecifyKind(model.ScheduledDate, DateTimeKind.Utc),
                EYDUserId = user.Id,
                AssessorUserId = model.IsInternalAssessor ? model.AssessorUserId : null,
                IsInternalAssessor = model.IsInternalAssessor,
                ExternalAssessorName = model.IsInternalAssessor ? null : model.ExternalAssessorName,
                ExternalAssessorEmail = model.IsInternalAssessor ? null : model.ExternalAssessorEmail,
                ExternalAssessorInstitution = model.IsInternalAssessor ? null : model.ExternalAssessorInstitution,
                Status = "Invited", // SLE is created and sent to assessor
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Set location field based on SLE type
            switch (model.SLEType)
            {
                case "CBD":
                case "DOPS":
                case "DOPSSim":
                case "MiniCEX":
                    sle.Location = model.Location;
                    break;
                case "DENTL":
                    sle.Setting = model.Setting;
                    break;
                case "DtCT":
                    sle.Audience = model.Audience;
                    sle.AudienceSetting = model.AudienceSetting;
                    break;
            }

            _context.SLEs.Add(sle);
            await _context.SaveChangesAsync();

            // Create EPA mappings
            Console.WriteLine($"DEBUG: Creating EPA mappings for {model.SelectedEPAIds.Count} EPAs");
            foreach (var epaId in model.SelectedEPAIds)
            {
                Console.WriteLine($"DEBUG: Creating mapping for EPA ID: {epaId}");
                var mapping = new EPAMapping
                {
                    EPAId = epaId,
                    EntityType = "SLE",
                    EntityId = sle.Id,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EPAMappings.Add(mapping);
            }

            await _context.SaveChangesAsync();

            // Generate external assessor token if needed
            if (!model.IsInternalAssessor)
            {
                await GenerateExternalAssessorToken(sle);
                TempData["Success"] = $"SLE ({GetSLETypeName(model.SLEType)}) created successfully. External assessment link generated.";
            }
            else
            {
                TempData["Success"] = $"SLE ({GetSLETypeName(model.SLEType)}) created and sent to assessor successfully.";
            }
            return RedirectToAction("Details", new { id = sle.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var sle = await _context.SLEs
                .Include(s => s.EYDUser)
                .Include(s => s.AssessorUser)
                .Include(s => s.EPAMappings)
                    .ThenInclude(em => em.EPA)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sle == null)
                return NotFound();

            // Check access permissions
            var hasAccess = sle.EYDUserId == user.Id || 
                           sle.AssessorUserId == user.Id ||
                           await HasViewPermission(user, sle);

            if (!hasAccess)
                return Forbid();

            return View(sle);
        }

        private async Task<bool> CanUserCreateSLE(ApplicationUser user)
        {
            // Users in "EYD" or "Superuser" roles can create SLEs
            return await _userManager.IsInRoleAsync(user, "EYD") || 
                   await _userManager.IsInRoleAsync(user, "Superuser");
        }

        private async Task<List<ApplicationUser>> GetAvailableAssessorsInArea(ApplicationUser eydUser)
        {
            var assessors = new List<ApplicationUser>();

            // Get the current user (EYD) with their scheme and area information
            var eydWithScheme = await _context.Users
                .Include(u => u.Scheme)
                .ThenInclude(s => s!.Area)
                .FirstOrDefaultAsync(u => u.Id == eydUser.Id);

            if (eydWithScheme?.Scheme != null)
            {
                // Get ES users assigned to this EYD
                var assignedESUsers = await _context.EYDESAssignments
                    .Include(a => a.ESUser)
                    .Where(a => a.EYDUserId == eydUser.Id && a.IsActive)
                    .Select(a => a.ESUser)
                    .ToListAsync();

                assessors.AddRange(assignedESUsers);

                // Get assigned ES user IDs for exclusion
                var assignedESUserIds = assignedESUsers.Select(u => u.Id).ToList();

                // Get TPD users in the same scheme
                var tpdUsers = await _context.Users
                    .Where(u => u.Role == "TPD" && u.SchemeId == eydWithScheme.SchemeId)
                    .ToListAsync();

                assessors.AddRange(tpdUsers);

                // Get other ES users in the same area (for broader selection)
                var areaESUsers = await _context.Users
                    .Where(u => u.Role == "ES" && u.AreaId == eydWithScheme.Scheme.AreaId && !assignedESUserIds.Contains(u.Id))
                    .ToListAsync();

                assessors.AddRange(areaESUsers);
            }

            return assessors
                .Distinct()
                .OrderBy(u => u.DisplayName)
                .ToList();
        }

        private List<(string Code, string Name)> GetAvailableSLETypes()
        {
            return SLETypes.TypeNames.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        private string GetSLETypeName(string sleType)
        {
            var types = GetAvailableSLETypes();
            return types.FirstOrDefault(t => t.Code == sleType).Name ?? sleType;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableEPAs(string sleType)
        {
            if (string.IsNullOrEmpty(sleType))
            {
                return Json(new { success = false, message = "SLE type is required" });
            }

            try
            {
                // Get all active EPAs
                var epas = await _context.EPAs
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Code)
                    .Select(e => new {
                        id = e.Id,
                        code = e.Code,
                        title = e.Title,
                        description = e.Description
                    })
                    .ToListAsync();

                // Get validation rules for this SLE type
                var singleEPATypes = new[] { "MiniCEX", "DOPS", "DOPSSim" };
                var maxSelections = singleEPATypes.Contains(sleType) ? 1 : 2;
                var requiresSelection = true; // All SLE types require EPA selection

                return Json(new { 
                    success = true, 
                    epas = epas,
                    maxSelections = maxSelections,
                    requiresSelection = requiresSelection
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading EPAs: " + ex.Message });
            }
        }

        private async Task GenerateExternalAssessorToken(SLE sle)
        {
            // Generate secure token for external assessor access
            var token = Guid.NewGuid().ToString();
            sle.ExternalAccessToken = token;
            sle.InvitationSentAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // TODO: Send email to external assessor with token link
            // await _emailService.SendExternalAssessorInvitationAsync(sle);
        }

        private async Task<bool> HasViewPermission(ApplicationUser user, SLE sle)
        {
            // Check if user has administrative access
            return await _userManager.IsInRoleAsync(user, "EYD-ASSESSOR-EYD");
        }

        // External Assessor Actions
        [AllowAnonymous]
        [HttpGet("external-assessment/{token}")]
        public async Task<IActionResult> ExternalAssessment(string token)
        {
            var sle = await _context.SLEs
                .Include(s => s.EYDUser)
                .FirstOrDefaultAsync(s => s.ExternalAccessToken == token);

            if (sle == null || string.IsNullOrEmpty(sle.ExternalAccessToken))
            {
                return NotFound("Invalid or expired assessment link.");
            }

            // Check if assessment is already completed
            if (sle.IsAssessmentCompleted)
            {
                return View("ExternalAssessmentCompleted", sle);
            }

            // Get linked EPAs
            var linkedEPAs = await _context.EPAMappings
                .Where(m => m.EntityType == "SLE" && m.EntityId == sle.Id)
                .Include(m => m.EPA)
                .Select(m => m.EPA)
                .ToListAsync();

            var viewModel = new SLEDetailViewModel
            {
                SLE = sle,
                LinkedEPAs = linkedEPAs,
                CanAssess = true,
                IsOwner = false
            };

            return View("ExternalAssessment", viewModel);
        }

        [AllowAnonymous]
        [HttpPost("external-assessment/{token}")]
        public async Task<IActionResult> SubmitExternalAssessment(string token, 
            string BehaviourFeedback, 
            string AgreedAction, 
            string AssessorPosition)
        {
            var sle = await _context.SLEs
                .Include(s => s.EYDUser)
                .FirstOrDefaultAsync(s => s.ExternalAccessToken == token);

            if (sle == null || string.IsNullOrEmpty(sle.ExternalAccessToken))
            {
                return NotFound("Invalid or expired assessment link.");
            }

            if (sle.IsAssessmentCompleted)
            {
                return View("ExternalAssessmentCompleted", sle);
            }

            // Update assessment with the 3 required fields
            sle.BehaviourFeedback = BehaviourFeedback;
            sle.AgreedAction = AgreedAction;
            sle.AssessorPosition = AssessorPosition;
            sle.AssessmentCompletedAt = DateTime.UtcNow;
            sle.IsAssessmentCompleted = true;
            sle.Status = "AssessmentCompleted";
            sle.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assessment submitted successfully. Thank you for your feedback.";
            return View("ExternalAssessmentCompleted", sle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitInternalAssessment(int id, 
            string BehaviourFeedback, string AgreedAction, string AssessorPosition)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var sle = await _context.SLEs
                .Include(s => s.EYDUser)
                .Include(s => s.AssessorUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sle == null) return NotFound();

            // Verify the current user is the assigned assessor
            if (sle.AssessorUserId != user.Id)
            {
                TempData["Error"] = "You are not authorized to complete this assessment.";
                return RedirectToAction("Index");
            }

            // Verify assessment is not already completed
            if (sle.IsAssessmentCompleted)
            {
                TempData["Error"] = "This assessment has already been completed.";
                return RedirectToAction("Details", new { id = sle.Id });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(BehaviourFeedback) || 
                string.IsNullOrWhiteSpace(AgreedAction) || 
                string.IsNullOrWhiteSpace(AssessorPosition))
            {
                TempData["Error"] = "All assessment fields are required.";
                return RedirectToAction("Details", new { id = sle.Id });
            }

            // Update SLE with assessment data
            sle.BehaviourFeedback = BehaviourFeedback.Trim();
            sle.AgreedAction = AgreedAction.Trim();
            sle.AssessorPosition = AssessorPosition.Trim();
            sle.AssessmentCompletedAt = DateTime.UtcNow;
            sle.IsAssessmentCompleted = true;
            sle.Status = "AssessmentCompleted";
            sle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Assessment submitted successfully. The EYD will be notified to complete their reflection.";
            return RedirectToAction("Details", new { id = sle.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReflection(int id, string ReflectionNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var sle = await _context.SLEs
                .Include(s => s.EYDUser)
                .Include(s => s.AssessorUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sle == null) return NotFound();

            // Verify the current user is the EYD who created this SLE
            if (sle.EYDUserId != user.Id)
            {
                TempData["Error"] = "You are not authorized to complete this reflection.";
                return RedirectToAction("Index");
            }

            // Verify assessment is completed but reflection is not
            if (!sle.IsAssessmentCompleted)
            {
                TempData["Error"] = "The assessment must be completed before adding reflection.";
                return RedirectToAction("Details", new { id = sle.Id });
            }

            if (sle.ReflectionCompletedAt.HasValue)
            {
                TempData["Error"] = "Reflection has already been completed for this SLE.";
                return RedirectToAction("Details", new { id = sle.Id });
            }

            // Validate reflection notes
            if (string.IsNullOrWhiteSpace(ReflectionNotes))
            {
                TempData["Error"] = "Reflection notes are required.";
                return RedirectToAction("Details", new { id = sle.Id });
            }

            // Update SLE with reflection
            sle.ReflectionNotes = ReflectionNotes.Trim();
            sle.ReflectionCompletedAt = DateTime.UtcNow;
            sle.Status = "ReflectionCompleted";
            sle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "SLE completed successfully! Your reflection has been saved.";
            return RedirectToAction("Details", new { id = sle.Id });
        }
    }
}
