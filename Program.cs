using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Helper method to convert Railway's postgres:// URL to connection string format
static string ConvertPostgresUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var db = uri.AbsolutePath.Trim('/');
    var user = uri.UserInfo.Split(':')[0];
    var password = uri.UserInfo.Split(':')[1];
    var host = uri.Host;
    var port = uri.Port;

    return $"Host={host};Port={port};Database={db};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

var builder = WebApplication.CreateBuilder(args);

// Configure database based on environment and connection string availability
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Check if we have a PostgreSQL connection string
if (!string.IsNullOrEmpty(connectionString) && 
    (connectionString.Contains("Host=") || connectionString.Contains("Server=") || connectionString.StartsWith("postgres://")))
{
    // Use PostgreSQL (for public database or Railway)
    if (connectionString.StartsWith("postgres://"))
    {
        // Convert Railway-style postgres:// URL to connection string format
        connectionString = ConvertPostgresUrl(connectionString);
    }
    
    Console.WriteLine("Using PostgreSQL database");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else if (builder.Environment.IsProduction())
{
    // Production without connection string - check environment variables
    var envConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    if (!string.IsNullOrEmpty(envConnectionString))
    {
        if (envConnectionString.StartsWith("postgres://"))
        {
            envConnectionString = ConvertPostgresUrl(envConnectionString);
        }
        
        Console.WriteLine("Using PostgreSQL from environment variable");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(envConnectionString));
    }
    else
    {
        throw new InvalidOperationException("No database connection string found for production environment.");
    }
}
else
{
    // Use SQLite for development (fallback)
    Console.WriteLine("Using SQLite database for development");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=local_eyd.db"));
}

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure authentication paths to use custom Account controller
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

// Register custom services
builder.Services.AddScoped<IEPAService, EPAService>();

// Configure for Railway deployment
if (builder.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Add request logging for debugging
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

// app.UseHttpsRedirection(); // Temporarily disabled for debugging

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "superuser",
    pattern: "{controller=Superuser}/{action=Dashboard}/{id?}");

app.MapControllerRoute(
    name: "eyd_portfolio",
    pattern: "EYD/Portfolio/{userId?}",
    defaults: new { controller = "EYD", action = "Portfolio" });

app.MapControllerRoute(
    name: "eyd",
    pattern: "EYD/{action=Dashboard}/{id?}",
    defaults: new { controller = "EYD" });

app.MapControllerRoute(
    name: "tpd_dashboard",
    pattern: "TPD/Dashboard/{userId?}",
    defaults: new { controller = "TPD", action = "UserDashboard" });

app.MapControllerRoute(
    name: "tpd",
    pattern: "TPD/{action=Dashboard}/{id?}",
    defaults: new { controller = "TPD" });

app.MapControllerRoute(
    name: "es_dashboard", 
    pattern: "ES/Dashboard/{userId?}",
    defaults: new { controller = "ES", action = "UserDashboard" });

app.MapControllerRoute(
    name: "es",
    pattern: "ES/{action=Dashboard}/{id?}",
    defaults: new { controller = "ES" });

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

// Add a simple test endpoint
app.MapGet("/test", () => "Hello! The server is working!");

// Add role debug endpoint
app.MapGet("/debug-roles", async (HttpContext httpContext, IServiceProvider serviceProvider) =>
{
    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    var currentUser = await userManager.GetUserAsync(httpContext.User);
    if (currentUser == null)
    {
        return Results.Json(new { Error = "No user logged in" });
    }
    
    var userRoles = await userManager.GetRolesAsync(currentUser);
    var isInEYDRole = await userManager.IsInRoleAsync(currentUser, "EYD");
    var isInSuperuserRole = await userManager.IsInRoleAsync(currentUser, "Superuser");
    
    return Results.Json(new {
        Username = currentUser.UserName,
        DisplayName = currentUser.DisplayName,
        RoleFieldValue = currentUser.Role, // This is just a string field
        AssignedRoles = userRoles.ToList(), // These are the actual role assignments
        IsInEYDRole = isInEYDRole,
        IsInSuperuserRole = isInSuperuserRole,
        CanCreateSLE = isInEYDRole || isInSuperuserRole
    });
});

// Add role assignment endpoint to fix user roles (supports both GET and POST)
app.MapMethods("/assign-role", new[] { "GET", "POST" }, async (HttpContext httpContext, IServiceProvider serviceProvider) =>
{
    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    var currentUser = await userManager.GetUserAsync(httpContext.User);
    if (currentUser == null)
    {
        return Results.Json(new { Error = "No user logged in" });
    }
    
    // Assign the user to the role that matches their Role field
    var roleToAssign = currentUser.Role;
    if (!string.IsNullOrEmpty(roleToAssign))
    {
        // First, ensure the role exists in the system
        if (!await roleManager.RoleExistsAsync(roleToAssign))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleToAssign));
            if (!createRoleResult.Succeeded)
            {
                return Results.Json(new { 
                    Success = false, 
                    Message = $"Failed to create role {roleToAssign}", 
                    Errors = createRoleResult.Errors.Select(e => e.Description).ToList()
                });
            }
        }
        
        // Check if user is already in the role
        if (await userManager.IsInRoleAsync(currentUser, roleToAssign))
        {
            return Results.Json(new { 
                Success = true, 
                Message = $"User {currentUser.UserName} is already assigned to role {roleToAssign}",
                UserName = currentUser.UserName,
                AssignedRole = roleToAssign,
                AlreadyAssigned = true
            });
        }
        
        // Assign the user to the role
        var result = await userManager.AddToRoleAsync(currentUser, roleToAssign);
        if (result.Succeeded)
        {
            return Results.Json(new { 
                Success = true, 
                Message = $"Successfully assigned user {currentUser.UserName} to role {roleToAssign}",
                UserName = currentUser.UserName,
                AssignedRole = roleToAssign,
                AlreadyAssigned = false
            });
        }
        else
        {
            return Results.Json(new { 
                Success = false, 
                Message = "Failed to assign role", 
                Errors = result.Errors.Select(e => e.Description).ToList()
            });
        }
    }
    else
    {
        return Results.Json(new { 
            Success = false, 
            Message = "User has no role field value to assign" 
        });
    }
});

// Add bulk role assignment endpoint to fix all users at once
app.MapGet("/fix-all-roles", async (HttpContext httpContext, IServiceProvider serviceProvider) =>
{
    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    var results = new List<object>();
    
    // First, ensure all required roles exist
    var requiredRoles = new[] { "Admin", "EYD", "TPD", "ES", "Dean", "Superuser" };
    foreach (var roleName in requiredRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var createResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            results.Add(new { Action = "CreateRole", Role = roleName, Success = createResult.Succeeded });
        }
    }
    
    // Get all users from database
    var allUsers = await context.Users.ToListAsync();
    var fixedCount = 0;
    var alreadyAssignedCount = 0;
    var errorCount = 0;
    
    foreach (var user in allUsers)
    {
        if (!string.IsNullOrEmpty(user.Role))
        {
            // Check if user is already in the role
            if (await userManager.IsInRoleAsync(user, user.Role))
            {
                alreadyAssignedCount++;
                continue;
            }
            
            // Assign the user to their role
            var result = await userManager.AddToRoleAsync(user, user.Role);
            if (result.Succeeded)
            {
                fixedCount++;
                results.Add(new { 
                    Action = "AssignRole", 
                    User = user.UserName, 
                    Role = user.Role, 
                    Success = true 
                });
            }
            else
            {
                errorCount++;
                results.Add(new { 
                    Action = "AssignRole", 
                    User = user.UserName, 
                    Role = user.Role, 
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
        }
    }
    
    return Results.Json(new {
        Summary = new {
            TotalUsers = allUsers.Count,
            FixedUsers = fixedCount,
            AlreadyAssigned = alreadyAssignedCount,
            Errors = errorCount
        },
        Details = results
    });
});

// Add database test endpoint
app.MapGet("/test-db", async (ApplicationDbContext context) =>
{
    var users = await context.Users.Include(u => u.Area).Include(u => u.Scheme).ToListAsync();
    var schemes = await context.Schemes.Include(s => s.Area).ToListAsync();
    var eydAssignments = await context.EYDESAssignments.CountAsync();
    var tempAccesses = await context.TemporaryAccesses.CountAsync();
    
    return new {
        UserCount = users.Count,
        AreaCount = await context.Areas.CountAsync(),
        SchemeCount = schemes.Count,
        EYDESAssignmentsCount = eydAssignments,
        TemporaryAccessesCount = tempAccesses,
        Users = users.Select(u => new {
            u.UserName,
            u.DisplayName,
            u.Role,
            AreaName = u.Area?.Name ?? "No Area",
            u.AreaId,
            SchemeName = u.Scheme?.Name ?? "No Scheme",
            u.SchemeId
        }).ToList(),
        Schemes = schemes.Select(s => new {
            s.Name,
            AreaName = s.Area.Name,
            s.AreaId
        }).ToList()
    };
});

// Quick setup endpoint for demo data
app.MapPost("/setup-demo-data", async (ApplicationDbContext context) =>
{
    // FORCE RESET: First clear user assignments, then schemes, then assignments
    
    // Clear user scheme assignments
    var users = await context.Users.ToListAsync();
    foreach (var user in users)
    {
        user.SchemeId = null;
    }
    await context.SaveChangesAsync();
    
    // Clear EYD-ES assignments
    var existingAssignments = await context.EYDESAssignments.ToListAsync();
    context.EYDESAssignments.RemoveRange(existingAssignments);
    
    // Clear temporary access records
    var existingTempAccess = await context.TemporaryAccesses.ToListAsync();
    context.TemporaryAccesses.RemoveRange(existingTempAccess);
    
    await context.SaveChangesAsync();
    
    // Now clear schemes
    var existingSchemes = await context.Schemes.ToListAsync();
    context.Schemes.RemoveRange(existingSchemes);
    await context.SaveChangesAsync();
    
    // Create sample areas if they don't exist, or use existing ones
    var areas = await context.Areas.ToListAsync();
    if (!areas.Any())
    {
        var newAreas = new[]
        {
            new Area { Name = "North West England" },
            new Area { Name = "South East England" }, 
            new Area { Name = "Scotland" },
            new Area { Name = "Wales" }
        };
        
        context.Areas.AddRange(newAreas);
        await context.SaveChangesAsync();
        areas = await context.Areas.ToListAsync();
    }
    
    // Create new schemes with correct geographic names
    var northWest = areas.First(a => a.Name == "North West England");
    var southEast = areas.First(a => a.Name == "South East England");
    
    var schemes = new[]
    {
        new Scheme { Name = "North West Group A", AreaId = northWest.Id },
        new Scheme { Name = "North West Group B", AreaId = northWest.Id },
        new Scheme { Name = "South East Group A", AreaId = southEast.Id },
        new Scheme { Name = "South East Group B", AreaId = southEast.Id }
    };
    
    context.Schemes.AddRange(schemes);
    await context.SaveChangesAsync();
    
    // Get all users and schemes for assignment
    var allUsers = await context.Users.ToListAsync();
    var allAreas = await context.Areas.ToListAsync();
    var allSchemes = await context.Schemes.ToListAsync();
    
    // Reset all user assignments first
    foreach (var user in allUsers)
    {
        user.AreaId = null;
        user.SchemeId = null;
    }
    
    // PHASE 2 ASSIGNMENT LOGIC:
    
    // 1. Assign Admins to Areas (keep existing logic)
    var adminUsers = allUsers.Where(u => u.Role == "Admin").ToList();
    for (int i = 0; i < adminUsers.Count && i < allAreas.Count; i++)
    {
        adminUsers[i].AreaId = allAreas[i].Id;
        adminUsers[i].SchemeId = null; // Admins are NOT assigned to schemes
    }
    
    // 2. Assign TPDs to Schemes (1:1 relationship)
    var tpdUsers = allUsers.Where(u => u.Role == "TPD" || u.Role == "Dean").ToList();
    for (int i = 0; i < tpdUsers.Count && i < allSchemes.Count; i++)
    {
        tpdUsers[i].SchemeId = allSchemes[i].Id;
        tpdUsers[i].AreaId = null; // TPDs are assigned to schemes, not areas directly
    }
    
    // 3. Assign EYDs to Schemes (distribute across available schemes)
    var eydUsers = allUsers.Where(u => u.Role == "EYD").ToList();
    for (int i = 0; i < eydUsers.Count; i++)
    {
        var schemeIndex = i % allSchemes.Count;
        eydUsers[i].SchemeId = allSchemes[schemeIndex].Id;
        eydUsers[i].AreaId = null; // EYDs are assigned to schemes, not areas directly
    }
    
    // 4. Assign ES users to first area (they'll be assigned to specific EYDs via EYDESAssignment table)
    var esUsers = allUsers.Where(u => u.Role == "ES").ToList();
    foreach (var esUser in esUsers)
    {
        esUser.AreaId = allAreas.Any() ? allAreas[0].Id : null;
        esUser.SchemeId = null; // ES users are not assigned to schemes directly
    }
    
    // 5. Leave Dean users without scheme assignment (they use search functionality)
    var deanUsers = allUsers.Where(u => u.Role == "Dean").ToList();
    foreach (var deanUser in deanUsers)
    {
        deanUser.AreaId = null; // Deans can search across all areas
        deanUser.SchemeId = null; // Deans are not assigned to specific schemes
    }
    
    await context.SaveChangesAsync();
    
    // 6. Create sample ES-EYD assignments
    var assignmentCount = 0;
    if (esUsers.Any() && eydUsers.Any())
    {
        // Create new assignments - each ES assigned to 2-3 EYDs
        foreach (var esUser in esUsers)
        {
            var assignedEyds = eydUsers.Take(3).ToList(); // Assign first 3 EYDs to each ES
            foreach (var eydUser in assignedEyds)
            {
                var assignment = new EYDESAssignment
                {
                    ESUserId = esUser.Id,
                    EYDUserId = eydUser.Id,
                    AssignedDate = DateTime.UtcNow,
                    IsActive = true
                };
                context.EYDESAssignments.Add(assignment);
                assignmentCount++;
            }
        }
        
        await context.SaveChangesAsync();
    }
    
    return new { 
        Message = "Demo data reset and setup complete with Phase 2 assignment logic!",
        AreasCreated = await context.Areas.CountAsync(),
        SchemesCreated = await context.Schemes.CountAsync(),
        AdminsAssignedToAreas = adminUsers.Count,
        TPDsAssignedToSchemes = tpdUsers.Count,
        EYDsAssignedToSchemes = eydUsers.Count,
        ESUsersAssignedToArea = esUsers.Count,
        DeanUsersUnassigned = deanUsers.Count,
        ESEYDAssignmentsCreated = assignmentCount
    };
});

// Initialize database with admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedAdminAsync(services);
}


app.Run();
