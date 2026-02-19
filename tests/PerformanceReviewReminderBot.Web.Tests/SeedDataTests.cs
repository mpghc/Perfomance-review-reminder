using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Tests;

public class SeedDataTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void Initialize_EmptyDatabase_Seeds5Employees()
    {
        using var context = CreateInMemoryContext();

        SeedData.Initialize(context);

        Assert.Equal(5, context.Employees.Count());
    }

    [Fact]
    public void Initialize_EmptyDatabase_Seeds1TalentManager()
    {
        using var context = CreateInMemoryContext();

        SeedData.Initialize(context);

        var talentManagers = context.Employees
            .Where(e => e.Role == EmployeeRole.TalentManager)
            .ToList();
        Assert.Single(talentManagers);
        Assert.Equal("Bill", talentManagers[0].FullName);
    }

    [Fact]
    public void Initialize_EmptyDatabase_Seeds4Employees()
    {
        using var context = CreateInMemoryContext();

        SeedData.Initialize(context);

        var employees = context.Employees
            .Where(e => e.Role == EmployeeRole.Employee)
            .ToList();
        Assert.Equal(4, employees.Count);
    }

    [Fact]
    public void Initialize_EmptyDatabase_Seeds12TeammateRows()
    {
        using var context = CreateInMemoryContext();

        SeedData.Initialize(context);

        Assert.Equal(12, context.EmployeeTeammates.Count());
    }

    [Fact]
    public void Initialize_EmptyDatabase_Seeds1PerformanceReview()
    {
        using var context = CreateInMemoryContext();

        SeedData.Initialize(context);

        var reviews = context.PerformanceReviews.ToList();
        Assert.Single(reviews);
        Assert.Equal(ReviewStatus.Scheduled, reviews[0].Status);
    }

    [Fact]
    public void Initialize_EmptyDatabase_SeedsAtLeast2Notifications()
    {
        using var context = CreateInMemoryContext();

        SeedData.Initialize(context);

        Assert.True(context.Notifications.Count() >= 2);
    }

    [Fact]
    public void Initialize_CalledTwice_DoesNotDuplicateData()
    {
        using var context = CreateInMemoryContext();

        SeedData.Initialize(context);
        SeedData.Initialize(context);

        Assert.Equal(5, context.Employees.Count());
        Assert.Equal(12, context.EmployeeTeammates.Count());
        Assert.Equal(1, context.PerformanceReviews.Count());
    }
}
