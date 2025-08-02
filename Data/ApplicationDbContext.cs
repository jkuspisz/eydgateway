using EYDGateway.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EYDGateway.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Area> Areas { get; set; }
        public DbSet<Scheme> Schemes { get; set; }
        public DbSet<EYDESAssignment> EYDESAssignments { get; set; }
        public DbSet<TemporaryAccess> TemporaryAccesses { get; set; }
    }
}
