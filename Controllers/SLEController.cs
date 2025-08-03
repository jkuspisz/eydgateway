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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var sles = await _context.SLEs
                .Include(s => s.EYDUser)
                .Include(s => s.AssessorUser)
                .Include(s => s.EPAMappings)
                    .ThenInclude(em => em.EPA)
                .Where(s => s.EYDUserId == user.Id || s.AssessorUserId == user.Id)
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
                CanCreateSLE = await CanUserCreateSLE(user)
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

            var viewModel = new CreateSLEViewModel
            {
                SLEType = sleType ?? string.Empty,
                AvailableSLETypes = GetAvailableSLETypes(),
                AvailableAssessors = await GetAvailableAssessors()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSLEViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await CanUserCreateSLE(user))
            {
                TempData["Error"] = "You don't have permission to create SLEs.";
                return RedirectToAction("Index", "EYD");
            }

            // Validate EPA selection based on SLE type
            var isEPASelectionValid = await _epaService.ValidateSLEEPASelectionAsync(
                model.SLEType, model.SelectedEPAIds);
            
            if (!isEPASelectionValid)
            {
                var singleEPATypes = new[] { "MiniCEX", "DOPS", "DOPSSim" };
                var errorMessage = singleEPATypes.Contains(model.SLEType) 
                    ? "This SLE type requires exactly 1 EPA selection."
                    : "This SLE type requires 1-2 EPA selections.";
                ModelState.AddModelError("SelectedEPAIds", errorMessage);
            }

            // Validate assessor selection
            if (model.IsInternalAssessor && string.IsNullOrEmpty(model.AssessorUserId))
            {
                ModelState.AddModelError("AssessorUserId", "Please select an internal assessor.");
            }
            else if (!model.IsInternalAssessor)
            {
                if (string.IsNullOrEmpty(model.ExternalAssessorName))
                    ModelState.AddModelError("ExternalAssessorName", "External assessor name is required.");
                if (string.IsNullOrEmpty(model.ExternalAssessorEmail))
                    ModelState.AddModelError("ExternalAssessorEmail", "External assessor email is required.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableSLETypes = GetAvailableSLETypes();
                model.AvailableAssessors = await GetAvailableAssessors();
                return View(model);
            }

            var sle = new SLE
            {
                SLEType = model.SLEType,
                Title = model.Title,
                Description = model.Description,
                ScheduledDate = model.ScheduledDate,
                LearningObjectives = model.LearningObjectives,
                EYDUserId = user.Id,
                AssessorUserId = model.IsInternalAssessor ? model.AssessorUserId : null,
                ExternalAssessorName = model.IsInternalAssessor ? null : model.ExternalAssessorName,
                ExternalAssessorEmail = model.IsInternalAssessor ? null : model.ExternalAssessorEmail,
                ExternalAssessorInstitution = model.IsInternalAssessor ? null : model.ExternalAssessorInstitution,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
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
                case "DCT":
                    sle.Audience = model.Audience;
                    sle.AudienceSetting = model.AudienceSetting;
                    break;
            }

            _context.SLEs.Add(sle);
            await _context.SaveChangesAsync();

            // Create EPA mappings
            foreach (var epaId in model.SelectedEPAIds)
            {
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
            }

            TempData["Success"] = $"SLE ({GetSLETypeName(model.SLEType)}) created successfully.";
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
            // Users in "EYD" role can create SLEs
            return await _userManager.IsInRoleAsync(user, "EYD");
        }

        private async Task<List<ApplicationUser>> GetAvailableAssessors()
        {
            // Get users in "ASSESSOR" or "EYD" roles
            var assessorUsers = await _userManager.GetUsersInRoleAsync("ASSESSOR");
            var eydUsers = await _userManager.GetUsersInRoleAsync("EYD");
            
            return assessorUsers.Union(eydUsers)
                .Where(u => u.EmailConfirmed)
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
    }
}
