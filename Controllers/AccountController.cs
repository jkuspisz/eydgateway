using EYDGateway.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EYDGateway.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(username);

                // Redirect based on user role
                switch (user?.Role?.ToLower())
                {
                    case "superuser":
                        return RedirectToAction("Dashboard", "Superuser");
                    case "admin":
                        return RedirectToAction("Dashboard", "Admin");
                    case "tpd":
                    case "dean":
                        return RedirectToAction("Dashboard", "TPD");
                    case "eyd":
                        return RedirectToAction("Dashboard", "EYD");
                    case "es":
                        return RedirectToAction("Dashboard", "ES");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Error = "Invalid login";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
