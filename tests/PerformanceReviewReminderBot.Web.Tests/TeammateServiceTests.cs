using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Tests;

public class TeammateServiceTests
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

    private static (Employee tm, Employee empA, Employee empB) SeedTwoEmployees(AppDbContext context)
    {
        var tm = new Employee { FullName = "Boss", Email = "boss@test.com", Role = EmployeeRole.TalentManager };
        var empA = new Employee { FullName = "Alice", Email = "alice@test.com", Role = EmployeeRole.Employee };
        var empB = new Employee { FullName = "Bob", Email = "bob@test.com", Role = EmployeeRole.Employee };
        context.Employees.AddRange(tm, empA, empB);
        context.SaveChanges();

        return (tm, empA, empB);
    }

    [Fact]
    public async Task AddTeammateAsync_ValidPair_Creates2Rows()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, empB) = SeedTwoEmployees(context);
        var service = new TeammateService(context);

        await service.AddTeammateAsync(empA.Id, empB.Id);

        Assert.Equal(2, await context.EmployeeTeammates.CountAsync());
    }

    [Fact]
    public async Task RemoveTeammateAsync_ExistingPair_Removes2Rows()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, empB) = SeedTwoEmployees(context);
        var service = new TeammateService(context);
        await service.AddTeammateAsync(empA.Id, empB.Id);

        await service.RemoveTeammateAsync(empA.Id, empB.Id);

        Assert.Equal(0, await context.EmployeeTeammates.CountAsync());
    }

    [Fact]
    public async Task AddTeammateAsync_Self_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, _) = SeedTwoEmployees(context);
        var service = new TeammateService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddTeammateAsync(empA.Id, empA.Id));

        Assert.Contains("own teammate", ex.Message);
    }

    [Fact]
    public async Task AddTeammateAsync_Duplicate_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, empB) = SeedTwoEmployees(context);
        var service = new TeammateService(context);
        await service.AddTeammateAsync(empA.Id, empB.Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddTeammateAsync(empA.Id, empB.Id));

        Assert.Contains("already a teammate", ex.Message);
    }

    [Fact]
    public async Task AddTeammateAsync_NonExistentEmployee_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, _) = SeedTwoEmployees(context);
        var service = new TeammateService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddTeammateAsync(empA.Id, 999));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task AddTeammateAsync_TalentManager_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (tm, empA, _) = SeedTwoEmployees(context);
        var service = new TeammateService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddTeammateAsync(empA.Id, tm.Id));

        Assert.Contains("Talent Manager", ex.Message);
    }

    [Fact]
    public async Task AddTeammateAsync_TalentManagerAsSource_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (tm, empA, _) = SeedTwoEmployees(context);
        var service = new TeammateService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddTeammateAsync(tm.Id, empA.Id));

        Assert.Contains("Talent Manager", ex.Message);
    }

    [Fact]
    public async Task GetTeammatesAsync_AfterAdd_ReturnsBidirectional()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, empB) = SeedTwoEmployees(context);
        var service = new TeammateService(context);
        await service.AddTeammateAsync(empA.Id, empB.Id);

        var teammatesOfA = await service.GetTeammatesAsync(empA.Id);
        var teammatesOfB = await service.GetTeammatesAsync(empB.Id);

        Assert.Single(teammatesOfA);
        Assert.Equal(empB.Id, teammatesOfA[0].Id);
        Assert.Single(teammatesOfB);
        Assert.Equal(empA.Id, teammatesOfB[0].Id);
    }

    [Fact]
    public async Task RemoveTeammateAsync_NonExistent_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, empB) = SeedTwoEmployees(context);
        var service = new TeammateService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RemoveTeammateAsync(empA.Id, empB.Id));

        Assert.Contains("No teammate relationship", ex.Message);
    }

    [Fact]
    public async Task GetEligibleTeammatesAsync_ExcludesSelfAndExisting()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, empB) = SeedTwoEmployees(context);
        var empC = new Employee { FullName = "Carol", Email = "carol@test.com", Role = EmployeeRole.Employee };
        context.Employees.Add(empC);
        await context.SaveChangesAsync();
        var service = new TeammateService(context);
        await service.AddTeammateAsync(empA.Id, empB.Id);

        var eligible = await service.GetEligibleTeammatesAsync(empA.Id);

        Assert.Single(eligible);
        Assert.Equal(empC.Id, eligible[0].Id);
    }
}
