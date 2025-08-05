using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.Services;
using EYDGateway.ViewModels;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class LearningNeedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEPAService _epaService;

        public LearningNeedController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEPAService epaService)
        {
            _context = context;
            _userManager = userManager;
            _epaService = epaService;
        }

        // GET: LearningNeed
        public async Task<IActionResult> Index(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

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
                        return Forbid("You can only view Learning Need data for EYD users assigned to you.");
                    }
                }
                else if (currentUser.Role == "EYD" && targetUserId != currentUser.Id)
                {
                    return Forbid("You can only view your own Learning Need data.");
                }
            }

            var learningNeeds = await _context.LearningNeeds
                .Where(ln => ln.UserId == targetUserId)  // Use targetUserId instead of currentUser.Id
                .OrderByDescending(ln => ln.DateIdentified)
                .ToListAsync();

            return View(learningNeeds);
        }

        // GET: LearningNeed/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            // First, get the learning need to determine its owner
            var learningNeed = await _context.LearningNeeds
                .FirstOrDefaultAsync(ln => ln.Id == id);

            if (learningNeed == null)
                return NotFound();

            // Check authorization: EYD users can only view their own learning needs
            // ES users can view learning needs of their assigned EYD users
            if (currentUser.Role == "EYD" && learningNeed.UserId != currentUser.Id)
            {
                return Forbid();
            }
            else if (currentUser.Role == "ES")
            {
                // Verify ES user has permission to view this EYD user's data
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == learningNeed.UserId && 
                             assignment.IsActive);

                if (!isAssigned)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
            {
                return Forbid();
            }

            var viewModel = new LearningNeedDetailViewModel
            {
                LearningNeed = learningNeed,
                CanView = true,
                CanEdit = true,
                IsOwner = true,
                AuthorName = currentUser.DisplayName ?? currentUser.UserName ?? "Unknown",
                UserId = currentUser.Id
            };

            return View(viewModel);
        }

        // GET: LearningNeed/Create
        public IActionResult Create()
        {
            var viewModel = new CreateLearningNeedViewModel();
            return View(viewModel);
        }

        // POST: LearningNeed/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLearningNeedViewModel viewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            if (ModelState.IsValid)
            {
                var learningNeed = new LearningNeed
                {
                    UserId = currentUser.Id,
                    Name = viewModel.Name,
                    DateIdentified = DateTime.SpecifyKind(viewModel.DateIdentified, DateTimeKind.Utc),
                    LearningObjectives = viewModel.LearningObjectives,
                    HowToAddress = viewModel.HowToAddress,
                    WhenToMeet = DateTime.SpecifyKind(viewModel.WhenToMeet, DateTimeKind.Utc),
                    Priority = viewModel.Priority,
                    Status = LearningNeedStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    
                    // Include completion fields from the form (can be empty)
                    AchievedBy = viewModel.AchievedBy ?? string.Empty,
                    ReflectionOnMeeting = viewModel.ReflectionOnMeeting ?? string.Empty,
                    DateOfAchievement = viewModel.DateOfAchievement.HasValue 
                        ? DateTime.SpecifyKind(viewModel.DateOfAchievement.Value, DateTimeKind.Utc) 
                        : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
                };

                _context.Add(learningNeed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: LearningNeed/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            // First, get the learning need to determine its owner
            var learningNeed = await _context.LearningNeeds
                .FirstOrDefaultAsync(ln => ln.Id == id);

            if (learningNeed == null)
                return NotFound();

            // Check authorization: EYD users can only edit their own learning needs
            // ES users can edit learning needs of their assigned EYD users
            if (currentUser.Role == "EYD" && learningNeed.UserId != currentUser.Id)
            {
                return Forbid();
            }
            else if (currentUser.Role == "ES")
            {
                // Verify ES user has permission to edit this EYD user's data
                var isAssigned = await _context.EYDESAssignments
                    .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                             assignment.EYDUserId == learningNeed.UserId && 
                             assignment.IsActive);

                if (!isAssigned)
                {
                    return Forbid();
                }
            }
            else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
            {
                return Forbid();
            }

            var viewModel = new EditLearningNeedViewModel
            {
                Id = learningNeed.Id,
                Name = learningNeed.Name,
                DateIdentified = learningNeed.DateIdentified,
                LearningObjectives = learningNeed.LearningObjectives,
                HowToAddress = learningNeed.HowToAddress,
                WhenToMeet = learningNeed.WhenToMeet,
                Priority = learningNeed.Priority,
                Status = learningNeed.Status,
                AchievedBy = learningNeed.AchievedBy,
                ReflectionOnMeeting = learningNeed.ReflectionOnMeeting,
                DateOfAchievement = learningNeed.DateOfAchievement
            };

            return View(viewModel);
        }

        // POST: LearningNeed/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditLearningNeedViewModel viewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            if (id != viewModel.Id)
                return NotFound();

            var existingLearningNeed = await _context.LearningNeeds
                .FirstOrDefaultAsync(ln => ln.Id == id && ln.UserId == currentUser.Id);

            if (existingLearningNeed == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingLearningNeed.Name = viewModel.Name;
                    existingLearningNeed.DateIdentified = DateTime.SpecifyKind(viewModel.DateIdentified, DateTimeKind.Utc);
                    existingLearningNeed.LearningObjectives = viewModel.LearningObjectives;
                    existingLearningNeed.HowToAddress = viewModel.HowToAddress;
                    existingLearningNeed.WhenToMeet = DateTime.SpecifyKind(viewModel.WhenToMeet, DateTimeKind.Utc);
                    existingLearningNeed.Priority = viewModel.Priority;
                    existingLearningNeed.AchievedBy = viewModel.AchievedBy;
                    existingLearningNeed.ReflectionOnMeeting = viewModel.ReflectionOnMeeting;
                    existingLearningNeed.DateOfAchievement = DateTime.SpecifyKind(viewModel.DateOfAchievement, DateTimeKind.Utc);
                    existingLearningNeed.Status = viewModel.Status;
                    existingLearningNeed.UpdatedAt = DateTime.UtcNow;

                    // Handle submission workflow
                    if (viewModel.Status == LearningNeedStatus.Submitted && existingLearningNeed.SubmittedAt == null)
                    {
                        existingLearningNeed.SubmittedAt = DateTime.UtcNow;
                    }
                    else if (viewModel.Status == LearningNeedStatus.Draft)
                    {
                        existingLearningNeed.SubmittedAt = null;
                    }

                    // Handle completion workflow
                    if (viewModel.Status == LearningNeedStatus.Completed && existingLearningNeed.CompletedAt == null)
                    {
                        existingLearningNeed.CompletedAt = DateTime.UtcNow;
                        existingLearningNeed.CompletedByUserId = currentUser.Id;
                    }
                    else if (viewModel.Status != LearningNeedStatus.Completed)
                    {
                        existingLearningNeed.CompletedAt = null;
                        existingLearningNeed.CompletedByUserId = null;
                    }

                    _context.Update(existingLearningNeed);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LearningNeedExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: LearningNeed/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var learningNeed = await _context.LearningNeeds
                .FirstOrDefaultAsync(ln => ln.Id == id && ln.UserId == currentUser.Id);

            if (learningNeed == null)
            {
                return NotFound();
            }

            return View(learningNeed);
        }

        // POST: LearningNeed/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var learningNeed = await _context.LearningNeeds
                .FirstOrDefaultAsync(ln => ln.Id == id && ln.UserId == currentUser.Id);

            if (learningNeed != null)
            {
                _context.LearningNeeds.Remove(learningNeed);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: LearningNeed/Submit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var learningNeed = await _context.LearningNeeds
                .FirstOrDefaultAsync(ln => ln.Id == id && ln.UserId == currentUser.Id);

            if (learningNeed == null)
                return NotFound();

            if (learningNeed.Status == LearningNeedStatus.Draft)
            {
                learningNeed.Status = LearningNeedStatus.Submitted;
                learningNeed.SubmittedAt = DateTime.UtcNow;
                learningNeed.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: LearningNeed/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var learningNeed = await _context.LearningNeeds
                .FirstOrDefaultAsync(ln => ln.Id == id);

            if (learningNeed == null)
                return NotFound();

            // Only ES or TPD can mark as complete
            if (currentUser.Role == "EYD")
                return Forbid();

            if (learningNeed.Status == LearningNeedStatus.Submitted)
            {
                learningNeed.Status = LearningNeedStatus.Completed;
                learningNeed.CompletedAt = DateTime.UtcNow;
                learningNeed.CompletedByUserId = currentUser.Id;
                learningNeed.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool LearningNeedExists(int id)
        {
            return _context.LearningNeeds.Any(e => e.Id == id);
        }
    }
}
