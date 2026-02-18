using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Models;

namespace PerformanceReviewBot.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.PerformanceReviews)
                  .WithOne(r => r.Employee)
                  .HasForeignKey(r => r.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PerformanceReview>(entity =>
        {
            entity.HasKey(r => r.Id);
        });
    }
}
