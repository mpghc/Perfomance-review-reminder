using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data.Entities;

namespace PerformanceReviewBot.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<ReminderLog> ReminderLogs => Set<ReminderLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);

            // Self-referencing relationship for Manager
            entity.HasOne(e => e.Manager)
                .WithMany(e => e.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // PerformanceReview configuration
        modelBuilder.Entity<PerformanceReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(pr => pr.Employee)
                .WithMany(e => e.PerformanceReviews)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pr => new { pr.EmployeeId, pr.ReviewDate });
        });

        // Feedback configuration
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(f => f.Comments).HasMaxLength(2000);

            entity.HasOne(f => f.PerformanceReview)
                .WithMany(pr => pr.Feedbacks)
                .HasForeignKey(f => f.PerformanceReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Reviewer)
                .WithMany(e => e.FeedbacksGiven)
                .HasForeignKey(f => f.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure one feedback per reviewer per review
            entity.HasIndex(f => new { f.PerformanceReviewId, f.ReviewerId }).IsUnique();
        });

        // ReminderLog configuration
        modelBuilder.Entity<ReminderLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(rl => rl.Message).IsRequired().HasMaxLength(500);

            entity.HasOne(rl => rl.PerformanceReview)
                .WithMany(pr => pr.ReminderLogs)
                .HasForeignKey(rl => rl.PerformanceReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rl => rl.Employee)
                .WithMany(e => e.ReminderLogs)
                .HasForeignKey(rl => rl.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(rl => rl.SentDate);
        });
    }
}
