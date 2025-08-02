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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PostgreSQL-specific configurations
            
            // Configure ApplicationUser for PostgreSQL
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.DisplayName)
                    .HasColumnType("text")
                    .IsRequired();
                
                entity.Property(e => e.Role)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.AreaId)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.SchemeId)
                    .HasColumnType("integer");
            });

            // Configure Area
            modelBuilder.Entity<Area>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .IsRequired();
            });

            // Configure Scheme
            modelBuilder.Entity<Scheme>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.AreaId)
                    .HasColumnType("integer");
            });

            // Configure EYDESAssignment
            modelBuilder.Entity<EYDESAssignment>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.EYDUserId)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.ESUserId)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.AssignedDate)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.IsActive)
                    .HasColumnType("boolean");
            });

            // Configure TemporaryAccess
            modelBuilder.Entity<TemporaryAccess>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.RequestingUserId)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.TargetEYDUserId)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.Reason)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.RequestedDate)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.ApprovedDate)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.ExpiryDate)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.IsApproved)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.IsActive)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.ApprovedByUserId)
                    .HasColumnType("text");
            });
        }
    }
}
