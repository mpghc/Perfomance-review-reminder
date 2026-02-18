using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Employees;

public class EditModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public EditModel(IEmployeeService employeeService)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _employeeService.UpdateAsync(Employee);
        return RedirectToPage("Index");
    }
}
