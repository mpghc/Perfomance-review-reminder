using PerformanceReviewBot.Models;

namespace PerformanceReviewBot.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        if (context.Employees.Any())
            return;

        var employees = new[]
        {
            new Employee { FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Department = "Engineering", HireDate = new DateTime(2022, 3, 15) },
            new Employee { FirstName = "Bob", LastName = "Smith", Email = "bob@example.com", Department = "Marketing", HireDate = new DateTime(2021, 7, 1) },
            new Employee { FirstName = "Carol", LastName = "Williams", Email = "carol@example.com", Department = "Engineering", HireDate = new DateTime(2023, 1, 10) },
            new Employee { FirstName = "David", LastName = "Brown", Email = "david@example.com", Department = "HR", HireDate = new DateTime(2020, 11, 20) },
            new Employee { FirstName = "Eve", LastName = "Davis", Email = "eve@example.com", Department = "Finance", HireDate = new DateTime(2023, 6, 5) }
        };

        context.Employees.AddRange(employees);
        context.SaveChanges();

        var now = DateTime.Today;
        var reviews = new[]
        {
            new PerformanceReview { EmployeeId = employees[0].Id, ReviewDate = now.AddDays(10), Status = ReviewStatus.Scheduled, Notes = "Annual review" },
            new PerformanceReview { EmployeeId = employees[1].Id, ReviewDate = now.AddDays(-15), Status = ReviewStatus.Overdue, Notes = "Quarterly check-in" },
            new PerformanceReview { EmployeeId = employees[2].Id, ReviewDate = now.AddDays(5), Status = ReviewStatus.Scheduled, Notes = "Probation review" },
            new PerformanceReview { EmployeeId = employees[3].Id, ReviewDate = now.AddDays(-30), Status = ReviewStatus.Completed, Notes = "Mid-year review" },
            new PerformanceReview { EmployeeId = employees[4].Id, ReviewDate = now.AddDays(-5), Status = ReviewStatus.Overdue, Notes = "First-year review" },
            new PerformanceReview { EmployeeId = employees[0].Id, ReviewDate = now.AddDays(20), Status = ReviewStatus.Scheduled, Notes = "Project milestone review" }
        };

        context.PerformanceReviews.AddRange(reviews);
        context.SaveChanges();
    }
}
