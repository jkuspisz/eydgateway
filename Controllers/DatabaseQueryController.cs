using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EYDGateway.Data;
using EYDGateway.Models;

namespace EYDGateway.Controllers
{
    public class DatabaseQueryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatabaseQueryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CheckUsers()
        {
            var users = await _context.Users
                .Include(u => u.Area)
                .Select(u => new {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.DisplayName,
                    u.Role,
                    u.AreaId,
                    AreaName = u.Area != null ? u.Area.Name : "No Area"
                })
                .ToListAsync();

            var areas = await _context.Areas
                .Include(a => a.Schemes)
                .Select(a => new {
                    a.Id,
                    a.Name,
                    SchemeCount = a.Schemes.Count,
                    Schemes = a.Schemes.Select(s => s.Name).ToList()
                })
                .ToListAsync();

            var result = new
            {
                Users = users,
                Areas = areas,
                UserCount = users.Count,
                AreaCount = areas.Count
            };

            return Json(result);
        }
    }
}
