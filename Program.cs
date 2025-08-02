using EYDGateway.Data;
using EYDGateway.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite database (for development)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

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
    name: "eyd",
    pattern: "EYD/{action=Dashboard}/{id?}",
    defaults: new { controller = "EYD" });

app.MapControllerRoute(
    name: "tpd",
    pattern: "TPD/{action=Dashboard}/{id?}",
    defaults: new { controller = "TPD" });

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
