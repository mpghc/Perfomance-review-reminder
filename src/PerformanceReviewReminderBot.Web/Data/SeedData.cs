using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Data;

/// <summary>
/// Seeds the database with demo data on first run.
/// Idempotent — safe to call on a non-empty database.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Populates the database with demo employees, teammate relationships,
    /// a sample performance review, and notification records.
    /// Does nothing if employees already exist.
    /// </summary>
    public static void Initialize(AppDbContext context)
    {
        if (context.Employees.Any())
        {
            return;
        }

        // 1. Seed employees — let auto-increment assign IDs.
        var bill = new Employee
        {
            FullName = "Bill",
            Email = "bill@company.com",
            Role = EmployeeRole.TalentManager,
            TalentManagerId = null
        };

        context.Employees.Add(bill);
        context.SaveChanges();

        var tom = new Employee
        {
            FullName = "Tom",
            Email = "tom@company.com",
            Role = EmployeeRole.Employee,
            TalentManagerId = bill.Id
        };

        var alice = new Employee
        {
            FullName = "Alice",
            Email = "alice@company.com",
            Role = EmployeeRole.Employee,
            TalentManagerId = bill.Id
        };

        var bob = new Employee
        {
            FullName = "Bob",
            Email = "bob@company.com",
            Role = EmployeeRole.Employee,
            TalentManagerId = bill.Id
        };

        var carol = new Employee
        {
            FullName = "Carol",
            Email = "carol@company.com",
            Role = EmployeeRole.Employee,
            TalentManagerId = bill.Id
        };

        context.Employees.AddRange(tom, alice, bob, carol);
        context.SaveChanges();

        // 2. Seed teammate relationships (bidirectional — both directions per pair).
        var teammates = new List<EmployeeTeammate>
        {
            new() { EmployeeId = tom.Id, TeammateId = alice.Id },
            new() { EmployeeId = alice.Id, TeammateId = tom.Id },

            new() { EmployeeId = tom.Id, TeammateId = bob.Id },
            new() { EmployeeId = bob.Id, TeammateId = tom.Id },

            new() { EmployeeId = tom.Id, TeammateId = carol.Id },
            new() { EmployeeId = carol.Id, TeammateId = tom.Id },

            new() { EmployeeId = alice.Id, TeammateId = bob.Id },
            new() { EmployeeId = bob.Id, TeammateId = alice.Id },

            new() { EmployeeId = alice.Id, TeammateId = carol.Id },
            new() { EmployeeId = carol.Id, TeammateId = alice.Id },

            new() { EmployeeId = bob.Id, TeammateId = carol.Id },
            new() { EmployeeId = carol.Id, TeammateId = bob.Id }
        };

        context.EmployeeTeammates.AddRange(teammates);
        context.SaveChanges();

        // 3. Seed one performance review: Tom's review, 14 days from now.
        var review = new PerformanceReview
        {
            EmployeeId = tom.Id,
            ReviewDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = ReviewStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        context.PerformanceReviews.Add(review);
        context.SaveChanges();

        // 4. Seed sample notifications for Alice and Bob about Tom's review.
        var notifications = new List<Notification>
        {
            new()
            {
                RecipientId = alice.Id,
                ReviewId = review.Id,
                Type = NotificationType.Reminder,
                Message = $"Reminder: Please submit feedback for Tom's upcoming performance review on {review.ReviewDate}.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                RecipientId = bob.Id,
                ReviewId = review.Id,
                Type = NotificationType.Reminder,
                Message = $"Reminder: Please submit feedback for Tom's upcoming performance review on {review.ReviewDate}.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Notifications.AddRange(notifications);
        context.SaveChanges();
    }
}
