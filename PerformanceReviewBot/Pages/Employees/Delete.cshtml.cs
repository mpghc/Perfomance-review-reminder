using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Employees;

public class DeleteModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public DeleteModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [BindProperty]
    public Employee Employee { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        Employee = employee;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _employeeService.DeleteAsync(Employee.Id);
        return RedirectToPage("Index");
    }
}
