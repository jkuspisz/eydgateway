using System.Collections.Generic;

namespace EYDGateway.Models
{
    public class AssignAdminViewModel
    {
        public Area Area { get; set; } = new Area();
        public List<ApplicationUser> AvailableAdmins { get; set; } = new List<ApplicationUser>();
    }

    public class SystemOverviewViewModel
    {
        public int TotalUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public int TotalAreas { get; set; }
        public int TotalSchemes { get; set; }
        public int UnassignedAdmins { get; set; }
        public int AreasWithoutAdmins { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
    }

    public class SuperuserUserManagementViewModel
    {
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<Area> Areas { get; set; } = new List<Area>();
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
    }

    public class SuperuserSchemeViewModel
    {
        public List<Area> Areas { get; set; } = new List<Area>();
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }

    public class SuperuserCreateUserViewModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public int? AreaId { get; set; }
        public int? SchemeId { get; set; }
        public List<Area> Areas { get; set; } = new List<Area>();
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
        public List<ApplicationUser> ExistingUsers { get; set; } = new List<ApplicationUser>();
    }

    public class EditUserDetailsViewModel
    {
        public string UserId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string CurrentRole { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public bool ChangePassword { get; set; } = false;
    }

    // Admin ViewModels for hierarchical user management
    public class AdminUserManagementViewModel
    {
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<Area> Areas { get; set; } = new List<Area>();
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
        public int? CurrentUserAreaId { get; set; }
    }

    public class AdminCreateUserViewModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public int? AreaId { get; set; }
        public int? SchemeId { get; set; }
        public List<Area> Areas { get; set; } = new List<Area>();
        public List<Scheme> Schemes { get; set; } = new List<Scheme>();
        public List<ApplicationUser> ExistingUsers { get; set; } = new List<ApplicationUser>();
    }
}
