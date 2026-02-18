using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data;
using PerformanceReviewBot.Data.Entities;

namespace PerformanceReviewBot.Services;

public class EmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .Include(e => e.Manager)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Manager)
            .Include(e => e.DirectReports)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Employee>> GetEmployeesByManagerAsync(int managerId)
    {
        return await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetManagersAsync()
    {
        return await _context.Employees
            .Where(e => e.IsManager)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }
}
