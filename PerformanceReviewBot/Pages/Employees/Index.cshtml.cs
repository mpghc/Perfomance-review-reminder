using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Employees;

public class IndexModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public IndexModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public List<Employee> Employees { get; set; } = new();

    public async Task OnGetAsync()
    {
        Employees = await _employeeService.GetAllAsync();
    }
}
