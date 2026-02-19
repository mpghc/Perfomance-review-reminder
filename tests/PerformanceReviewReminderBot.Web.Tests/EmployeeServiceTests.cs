using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Tests;

public class EmployeeServiceTests
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

    private static Employee CreateTalentManager(string name = "TM One", string email = "tm@test.com")
    {
        return new Employee
        {
            FullName = name,
            Email = email,
            Role = EmployeeRole.TalentManager
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmployeesOrderedByName()
    {
        using var context = CreateInMemoryContext();
        context.Employees.Add(new Employee { FullName = "Zara", Email = "zara@test.com", Role = EmployeeRole.Employee });
        context.Employees.Add(new Employee { FullName = "Alice", Email = "alice@test.com", Role = EmployeeRole.Employee });
        await context.SaveChangesAsync();
        var service = new EmployeeService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].FullName);
        Assert.Equal("Zara", result[1].FullName);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEmployee()
    {
        using var context = CreateInMemoryContext();
        var employee = new Employee { FullName = "Tom", Email = "tom@test.com", Role = EmployeeRole.Employee };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var service = new EmployeeService(context);

        var result = await service.GetByIdAsync(employee.Id);

        Assert.NotNull(result);
        Assert.Equal("Tom", result.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var service = new EmployeeService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidData_CreatesAndReturnsEmployee()
    {
        using var context = CreateInMemoryContext();
        var service = new EmployeeService(context);
        var employee = new Employee { FullName = "New Person", Email = "new@test.com", Role = EmployeeRole.Employee };

        var result = await service.CreateAsync(employee);

        Assert.True(result.Id > 0);
        Assert.Equal(1, await context.Employees.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new EmployeeService(context);
        var employee = new Employee { FullName = "", Email = "test@test.com", Role = EmployeeRole.Employee };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(employee));

        Assert.Contains("FullName", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_EmptyEmail_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new EmployeeService(context);
        var employee = new Employee { FullName = "Valid Name", Email = "  ", Role = EmployeeRole.Employee };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(employee));

        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ValidData_UpdatesEmployee()
    {
        using var context = CreateInMemoryContext();
        var employee = new Employee { FullName = "Original", Email = "orig@test.com", Role = EmployeeRole.Employee };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var service = new EmployeeService(context);

        var updated = new Employee
        {
            Id = employee.Id,
            FullName = "Updated",
            Email = "updated@test.com",
            Role = EmployeeRole.Employee
        };
        var result = await service.UpdateAsync(updated);

        Assert.Equal("Updated", result.FullName);
        Assert.Equal("updated@test.com", result.Email);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new EmployeeService(context);
        var employee = new Employee { Id = 999, FullName = "Ghost", Email = "ghost@test.com" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(employee));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_NoDependencies_DeletesEmployee()
    {
        using var context = CreateInMemoryContext();
        var employee = new Employee { FullName = "Deletable", Email = "del@test.com", Role = EmployeeRole.Employee };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var service = new EmployeeService(context);

        await service.DeleteAsync(employee.Id);

        Assert.Equal(0, await context.Employees.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_EmployeeWithReviews_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var employee = new Employee { FullName = "Reviewed", Email = "rev@test.com", Role = EmployeeRole.Employee };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        context.PerformanceReviews.Add(new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateOnly.FromDateTime(DateTime.Today),
            Status = ReviewStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = new EmployeeService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(employee.Id));

        Assert.Contains("performance review", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_TalentManagerWithAssignedEmployees_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var tm = CreateTalentManager();
        context.Employees.Add(tm);
        await context.SaveChangesAsync();
        context.Employees.Add(new Employee
        {
            FullName = "Subordinate",
            Email = "sub@test.com",
            Role = EmployeeRole.Employee,
            TalentManagerId = tm.Id
        });
        await context.SaveChangesAsync();
        var service = new EmployeeService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(tm.Id));

        Assert.Contains("assigned employee", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new EmployeeService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(999));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task GetTalentManagersAsync_ReturnsOnlyTalentManagers()
    {
        using var context = CreateInMemoryContext();
        context.Employees.Add(CreateTalentManager());
        context.Employees.Add(new Employee { FullName = "Regular", Email = "reg@test.com", Role = EmployeeRole.Employee });
        await context.SaveChangesAsync();
        var service = new EmployeeService(context);

        var result = await service.GetTalentManagersAsync();

        Assert.Single(result);
        Assert.Equal(EmployeeRole.TalentManager, result[0].Role);
    }
}
