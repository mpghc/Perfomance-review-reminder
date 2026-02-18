using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data.Entities;

namespace PerformanceReviewBot.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        // Check if already seeded
        if (await context.Employees.AnyAsync())
        {
            return;
        }

        // Seed employees
        var manager1 = new Employee
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@company.com",
            Department = "Engineering",
            IsManager = true,
            CreatedDate = DateTime.UtcNow
        };

        var manager2 = new Employee
        {
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob.smith@company.com",
            Department = "Product",
            IsManager = true,
            CreatedDate = DateTime.UtcNow
        };

        context.Employees.AddRange(manager1, manager2);
        await context.SaveChangesAsync();

        var employee1 = new Employee
        {
            FirstName = "Charlie",
            LastName = "Brown",
            Email = "charlie.brown@company.com",
            Department = "Engineering",
            IsManager = false,
            ManagerId = manager1.Id,
            CreatedDate = DateTime.UtcNow
        };

        var employee2 = new Employee
        {
            FirstName = "Diana",
            LastName = "Prince",
            Email = "diana.prince@company.com",
            Department = "Engineering",
            IsManager = false,
            ManagerId = manager1.Id,
            CreatedDate = DateTime.UtcNow
        };

        var employee3 = new Employee
        {
            FirstName = "Eve",
            LastName = "Davis",
            Email = "eve.davis@company.com",
            Department = "Product",
            IsManager = false,
            ManagerId = manager2.Id,
            CreatedDate = DateTime.UtcNow
        };

        context.Employees.AddRange(employee1, employee2, employee3);
        await context.SaveChangesAsync();

        // Seed performance reviews for current month
        var currentMonth = DateTime.UtcNow;
        var reviews = new List<PerformanceReview>
        {
            new PerformanceReview
            {
                EmployeeId = employee1.Id,
                ReviewDate = new DateTime(currentMonth.Year, currentMonth.Month, 15),
                Status = ReviewStatus.Scheduled,
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new PerformanceReview
            {
                EmployeeId = employee2.Id,
                ReviewDate = new DateTime(currentMonth.Year, currentMonth.Month, 20),
                Status = ReviewStatus.InProgress,
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            },
            new PerformanceReview
            {
                EmployeeId = employee3.Id,
                ReviewDate = new DateTime(currentMonth.Year, currentMonth.Month, 25),
                Status = ReviewStatus.Scheduled,
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            }
        };

        context.PerformanceReviews.AddRange(reviews);
        await context.SaveChangesAsync();

        // Seed some feedback (partial to demonstrate missing feedback)
        var feedback = new Feedback
        {
            PerformanceReviewId = reviews[1].Id,
            ReviewerId = manager1.Id,
            Comments = "Great work on the recent project deliverables.",
            Rating = 5,
            IsManagerFeedback = true,
            SubmittedDate = DateTime.UtcNow.AddDays(-2)
        };

        context.Feedbacks.Add(feedback);
        await context.SaveChangesAsync();
    }
}
