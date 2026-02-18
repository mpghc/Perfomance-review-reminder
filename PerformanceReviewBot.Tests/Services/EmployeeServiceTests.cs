using PerformanceReviewBot.Data.Entities;
using PerformanceReviewBot.Services;
using PerformanceReviewBot.Tests.Helpers;
using Xunit;

namespace PerformanceReviewBot.Tests.Services;

public class EmployeeServiceTests : IDisposable
{
    private readonly Data.AppDbContext _context;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _service = new EmployeeService(_context);
    }

    public void Dispose()
    {
        TestDbContextFactory.DisposeContext(_context);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldAddEmployee()
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Department = "IT",
            IsManager = false
        };

        // Act
        var result = await _service.CreateEmployeeAsync(employee);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ShouldReturnAllEmployees()
    {
        // Arrange
        await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Alice", 
            LastName = "Smith", 
            Email = "alice@test.com", 
            Department = "HR", 
            IsManager = true 
        });
        await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Bob", 
            LastName = "Johnson", 
            Email = "bob@test.com", 
            Department = "IT", 
            IsManager = false 
        });

        // Act
        var result = await _service.GetAllEmployeesAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldReturnCorrectEmployee()
    {
        // Arrange
        var employee = await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Test", 
            LastName = "User", 
            Email = "test@test.com", 
            Department = "Sales", 
            IsManager = false 
        });

        // Act
        var result = await _service.GetEmployeeByIdAsync(employee.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
        Assert.Equal("Test", result.FirstName);
    }

    [Fact]
    public async Task GetManagersAsync_ShouldReturnOnlyManagers()
    {
        // Arrange
        await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Manager1", 
            LastName = "Test", 
            Email = "mgr1@test.com", 
            Department = "IT", 
            IsManager = true 
        });
        await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Employee1", 
            LastName = "Test", 
            Email = "emp1@test.com", 
            Department = "IT", 
            IsManager = false 
        });
        await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Manager2", 
            LastName = "Test", 
            Email = "mgr2@test.com", 
            Department = "HR", 
            IsManager = true 
        });

        // Act
        var result = await _service.GetManagersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.True(e.IsManager));
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ShouldUpdateEmployee()
    {
        // Arrange
        var employee = await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Original", 
            LastName = "Name", 
            Email = "original@test.com", 
            Department = "IT", 
            IsManager = false 
        });

        // Act
        employee.FirstName = "Updated";
        employee.Department = "HR";
        await _service.UpdateEmployeeAsync(employee);

        var updated = await _service.GetEmployeeByIdAsync(employee.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.FirstName);
        Assert.Equal("HR", updated.Department);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldRemoveEmployee()
    {
        // Arrange
        var employee = await _service.CreateEmployeeAsync(new Employee 
        { 
            FirstName = "Delete", 
            LastName = "Me", 
            Email = "delete@test.com", 
            Department = "IT", 
            IsManager = false 
        });

        // Act
        await _service.DeleteEmployeeAsync(employee.Id);
        var result = await _service.GetEmployeeByIdAsync(employee.Id);

        // Assert
        Assert.Null(result);
    }
}
