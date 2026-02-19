using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Provides CRUD operations for <see cref="Employee"/> entities.
/// Uses AppDbContext directly (no repository abstraction per project rules).
/// </summary>
public class EmployeeService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Returns all employees, including their TalentManager navigation property.
    /// </summary>
    /// <example>
    /// <code>
    /// var employees = await employeeService.GetAllAsync();
    /// </code>
    /// </example>
    public async Task<List<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.TalentManager)
            .OrderBy(e => e.FullName)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Returns the employee with the specified <paramref name="id"/>,
    /// or <c>null</c> if not found.
    /// </summary>
    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.TalentManager)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Creates a new employee after validating required fields.
    /// </summary>
    /// <param name="employee">The employee entity to create.</param>
    /// <returns>The created employee with its generated Id.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Employee.FullName"/> or <see cref="Employee.Email"/> is empty.
    /// </exception>
    public async Task<Employee> CreateAsync(Employee employee)
    {
        ValidateRequiredFields(employee);

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return employee;
    }

    /// <summary>
    /// Updates an existing employee after validating required fields.
    /// </summary>
    /// <param name="employee">The employee entity with updated values.</param>
    /// <returns>The updated employee.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Employee.FullName"/> or <see cref="Employee.Email"/> is empty,
    /// or when the employee does not exist.
    /// </exception>
    public async Task<Employee> UpdateAsync(Employee employee)
    {
        ValidateRequiredFields(employee);

        var existing = await _context.Employees.FindAsync(employee.Id);

        if (existing is null)
        {
            throw new InvalidOperationException($"Employee with Id {employee.Id} not found.");
        }

        existing.FullName = employee.FullName;
        existing.Email = employee.Email;
        existing.Role = employee.Role;
        existing.TalentManagerId = employee.TalentManagerId;

        await _context.SaveChangesAsync();

        return existing;
    }

    /// <summary>
    /// Deletes the employee with the specified <paramref name="id"/>.
    /// Validates that the employee has no associated reviews and,
    /// if a Talent Manager, has no assigned employees.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the employee has reviews, has managed employees, or does not exist.
    /// </exception>
    public async Task DeleteAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.PerformanceReviews)
            .Include(e => e.ManagedEmployees)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            throw new InvalidOperationException($"Employee with Id {id} not found.");
        }

        if (employee.PerformanceReviews.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete employee '{employee.FullName}' because they have {employee.PerformanceReviews.Count} performance review(s).");
        }

        if (employee.ManagedEmployees.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete Talent Manager '{employee.FullName}' because they have {employee.ManagedEmployees.Count} assigned employee(s).");
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Returns all employees with the TalentManager role, for dropdown lists.
    /// </summary>
    public async Task<List<Employee>> GetTalentManagersAsync()
    {
        return await _context.Employees
            .Where(e => e.Role == EmployeeRole.TalentManager)
            .OrderBy(e => e.FullName)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Validates that FullName and Email are not empty.
    /// </summary>
    private static void ValidateRequiredFields(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            throw new InvalidOperationException($"{nameof(Employee.FullName)} is required.");
        }

        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            throw new InvalidOperationException($"{nameof(Employee.Email)} is required.");
        }
    }
}
