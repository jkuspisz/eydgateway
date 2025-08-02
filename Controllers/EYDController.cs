using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class EYDController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EYDController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // EYD users can only view their assigned schemes and upload certificates
            var currentUser = await _context.Users
                .Include(u => u.Area)
                    .ThenInclude(a => a.Schemes)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser?.Role != "EYD")
            {
                return Unauthorized();
            }

            var viewModel = new EYDDashboardViewModel
            {
                UserName = currentUser.DisplayName ?? currentUser.UserName,
                AssignedArea = currentUser.Area?.Name ?? "No Area Assigned",
                AssignedSchemes = currentUser.Area?.Schemes?.Select(s => s.Name).ToList() ?? new List<string>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadCertificate(IFormFile certificateFile, string schemeName)
        {
            if (certificateFile == null || certificateFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return RedirectToAction("Dashboard");
            }

            // Here you would implement file upload logic
            // For now, just simulate successful upload
            TempData["SuccessMessage"] = $"Certificate for {schemeName} uploaded successfully.";
            
            return RedirectToAction("Dashboard");
        }
    }

    public class EYDDashboardViewModel
    {
        public string UserName { get; set; }
        public string AssignedArea { get; set; }
        public List<string> AssignedSchemes { get; set; } = new List<string>();
    }
}
