using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Data;

/// <summary>
/// Application database context. Configures all entity relationships,
/// composite keys, and indexes for the Performance Review Reminder domain.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeTeammate> EmployeeTeammates => Set<EmployeeTeammate>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureEmployee(modelBuilder);
        ConfigureEmployeeTeammate(modelBuilder);
        ConfigurePerformanceReview(modelBuilder);
        ConfigureFeedback(modelBuilder);
        ConfigureNotification(modelBuilder);
    }

    /// <summary>
    /// Configures the Employee entity: self-referencing TalentManager FK,
    /// property constraints, and email index.
    /// </summary>
    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Role)
                .IsRequired();

            // Self-referencing FK: Employee → TalentManager
            // Restrict delete to prevent cascade-delete cycles in SQLite.
            entity.HasOne(e => e.TalentManager)
                .WithMany(e => e.ManagedEmployees)
                .HasForeignKey(e => e.TalentManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Email);
        });
    }

    /// <summary>
    /// Configures the EmployeeTeammate join table: composite PK and
    /// two separate FK relationships to Employee.
    /// </summary>
    private static void ConfigureEmployeeTeammate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeTeammate>(entity =>
        {
            entity.HasKey(et => new { et.EmployeeId, et.TeammateId });

            // FK: EmployeeTeammate.EmployeeId → Employee
            entity.HasOne(et => et.Employee)
                .WithMany(e => e.EmployeeTeammates)
                .HasForeignKey(et => et.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK: EmployeeTeammate.TeammateId → Employee
            entity.HasOne(et => et.Teammate)
                .WithMany(e => e.TeammateOf)
                .HasForeignKey(et => et.TeammateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the PerformanceReview entity: FK to Employee,
    /// composite index on (EmployeeId, ReviewDate).
    /// </summary>
    private static void ConfigurePerformanceReview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PerformanceReview>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.ReviewDate)
                .IsRequired();

            entity.Property(r => r.Status)
                .IsRequired();

            entity.Property(r => r.CreatedAt)
                .IsRequired();

            entity.HasOne(r => r.Employee)
                .WithMany(e => e.PerformanceReviews)
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => new { r.EmployeeId, r.ReviewDate });
        });
    }

    /// <summary>
    /// Configures the Feedback entity: FKs to PerformanceReview and Author (Employee),
    /// unique index on (ReviewId, AuthorId).
    /// </summary>
    private static void ConfigureFeedback(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Content)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(f => f.SubmittedAt)
                .IsRequired();

            entity.HasOne(f => f.Review)
                .WithMany(r => r.Feedbacks)
                .HasForeignKey(f => f.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Author)
                .WithMany()
                .HasForeignKey(f => f.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(f => new { f.ReviewId, f.AuthorId })
                .IsUnique();
        });
    }

    /// <summary>
    /// Configures the Notification entity: FK to Recipient (Employee),
    /// optional FK to PerformanceReview, index on (RecipientId, IsRead).
    /// </summary>
    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(n => n.IsRead)
                .IsRequired();

            entity.Property(n => n.CreatedAt)
                .IsRequired();

            entity.HasOne(n => n.Recipient)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional FK to PerformanceReview (nullable ReviewId).
            entity.HasOne(n => n.Review)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.ReviewId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(n => new { n.RecipientId, n.IsRead });
        });
    }
}
