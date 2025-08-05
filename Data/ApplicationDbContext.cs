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
        public DbSet<EPA> EPAs { get; set; }
        public DbSet<EPAMapping> EPAMappings { get; set; }
        public DbSet<SLE> SLEs { get; set; }
        
        // User-scoped portfolio data models
        public DbSet<PortfolioReflection> Reflections { get; set; }
        public DbSet<DocumentUpload> DocumentUploads { get; set; }
        public DbSet<LearningLog> LearningLogs { get; set; }
        public DbSet<ClinicalExperienceLog> ClinicalExperienceLogs { get; set; }
        public DbSet<ProtectedLearningTime> ProtectedLearningTimes { get; set; }
        public DbSet<LearningNeed> LearningNeeds { get; set; }
        public DbSet<ESInduction> ESInductions { get; set; }
        public DbSet<ClinicalLog> ClinicalLogs { get; set; }
        public DbSet<SignificantEvent> SignificantEvents { get; set; }

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

            // Configure EPA
            modelBuilder.Entity<EPA>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.Code)
                    .HasColumnType("varchar(10)")
                    .IsRequired();
                    
                entity.Property(e => e.Title)
                    .HasColumnType("varchar(200)")
                    .IsRequired();
                    
                entity.Property(e => e.Description)
                    .HasColumnType("varchar(1000)");
                    
                entity.Property(e => e.IsActive)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.HasIndex(e => e.Code)
                    .IsUnique();
            });

            // Configure EPAMapping
            modelBuilder.Entity<EPAMapping>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.EPAId)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.EntityType)
                    .HasColumnType("varchar(50)")
                    .IsRequired();
                    
                entity.Property(e => e.EntityId)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.UserId)
                    .HasColumnType("varchar(450)")
                    .IsRequired();
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.HasOne(e => e.EPA)
                    .WithMany(e => e.EPAMappings)
                    .HasForeignKey(e => e.EPAId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => new { e.EntityType, e.EntityId, e.EPAId })
                    .IsUnique();
            });

            // Configure SLE
            modelBuilder.Entity<SLE>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.SLEType)
                    .HasColumnType("varchar(50)")
                    .IsRequired();
                    
                entity.Property(e => e.EYDUserId)
                    .HasColumnType("varchar(450)")
                    .IsRequired();
                    
                entity.Property(e => e.Title)
                    .HasColumnType("varchar(200)")
                    .IsRequired();
                    
                entity.Property(e => e.Description)
                    .HasColumnType("varchar(1000)");
                    
                entity.Property(e => e.ScheduledDate)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.Location)
                    .HasColumnType("varchar(200)");
                    
                entity.Property(e => e.AssessorUserId)
                    .HasColumnType("varchar(450)");
                    
                entity.Property(e => e.ExternalAssessorName)
                    .HasColumnType("varchar(200)");
                    
                entity.Property(e => e.ExternalAssessorEmail)
                    .HasColumnType("varchar(255)");
                    
                entity.Property(e => e.ExternalAssessorInstitution)
                    .HasColumnType("varchar(100)");
                    
                entity.Property(e => e.ExternalAccessToken)
                    .HasColumnType("varchar(36)");
                    
                entity.Property(e => e.InvitationSentAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.AssessmentCompletedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.BehaviourFeedback)
                    .HasColumnType("varchar(2000)");
                    
                entity.Property(e => e.AgreedAction)
                    .HasColumnType("varchar(1000)");
                    
                entity.Property(e => e.AssessorPosition)
                    .HasColumnType("varchar(200)");
                    
                entity.Property(e => e.ReflectionNotes)
                    .HasColumnType("varchar(1000)");
                    
                entity.Property(e => e.ReflectionCompletedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.Status)
                    .HasColumnType("varchar(50)")
                    .HasDefaultValue("Draft");
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                // Relationships
                entity.HasOne(e => e.EYDUser)
                    .WithMany()
                    .HasForeignKey(e => e.EYDUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.AssessorUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssessorUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                // Indexes
                entity.HasIndex(e => e.EYDUserId);
                entity.HasIndex(e => e.AssessorUserId);
                entity.HasIndex(e => e.ExternalAccessToken)
                    .IsUnique();
                entity.HasIndex(e => e.SLEType);
                entity.HasIndex(e => e.Status);
            });

            // Configure ESInduction
            modelBuilder.Entity<ESInduction>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.EYDUserId)
                    .HasColumnType("varchar(450)")
                    .IsRequired();
                    
                entity.Property(e => e.ESUserId)
                    .HasColumnType("varchar(450)")
                    .IsRequired();
                    
                entity.Property(e => e.HasReadTransitionDocumentAndAgreedPDP)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.MeetingNotesAndComments)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.PlacementDescription)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.MeetingDate)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.IsCompleted)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.CompletedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.HasOne(e => e.EYDUser)
                    .WithMany()
                    .HasForeignKey(e => e.EYDUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.ESUser)
                    .WithMany()
                    .HasForeignKey(e => e.ESUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Each EYD user should have only one ES Induction record
                entity.HasIndex(e => e.EYDUserId)
                    .IsUnique();
                    
                entity.HasIndex(e => e.ESUserId);
            });

            // Configure SignificantEvent
            modelBuilder.Entity<SignificantEvent>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("integer");
                    
                entity.Property(e => e.UserId)
                    .HasColumnType("varchar(450)")
                    .IsRequired();
                    
                entity.Property(e => e.Title)
                    .HasColumnType("varchar(200)")
                    .IsRequired();
                    
                entity.Property(e => e.AccountOfExperience)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.AnalysisOfSituation)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.ReflectionOnEvent)
                    .HasColumnType("text")
                    .IsRequired();
                    
                entity.Property(e => e.IsLocked)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.ESSignedOff)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.ESSignedOffAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.ESUserId)
                    .HasColumnType("varchar(450)");
                    
                entity.Property(e => e.TPDSignedOff)
                    .HasColumnType("boolean");
                    
                entity.Property(e => e.TPDSignedOffAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.TPDUserId)
                    .HasColumnType("varchar(450)");
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.ESUser)
                    .WithMany()
                    .HasForeignKey(e => e.ESUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.TPDUser)
                    .WithMany()
                    .HasForeignKey(e => e.TPDUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ESUserId);
                entity.HasIndex(e => e.TPDUserId);
                entity.HasIndex(e => e.ESSignedOff);
                entity.HasIndex(e => e.TPDSignedOff);
            });

            // TODO: Add configurations for new user-scoped models after migration
            // PortfolioReflection, DocumentUpload, LearningLog, ClinicalExperienceLog
        }
    }
}
