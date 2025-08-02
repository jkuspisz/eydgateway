using EYDGateway.Models;
using Microsoft.AspNetCore.Identity;

namespace EYDGateway.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure roles
            string[] roles = new[] { "Superuser", "Admin", "Dean", "TPD", "ES", "EYD" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create initial admin user
            string adminEmail = "admin@site.com";
            string adminUser = "admin";
            string adminPwd = "Admin123!";

            var user = await userManager.FindByNameAsync(adminUser);
            if (user == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminUser,
                    Email = adminEmail,
                    DisplayName = "Admin",
                    Role = "Superuser"
                };
                var result = await userManager.CreateAsync(admin, adminPwd);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Superuser");
                }
            }
        }
    }
}
