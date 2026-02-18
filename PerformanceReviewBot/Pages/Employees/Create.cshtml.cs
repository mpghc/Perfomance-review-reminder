using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Employees;

public class CreateModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public CreateModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [BindProperty]
    public Employee Employee { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _employeeService.CreateAsync(Employee);
        return RedirectToPage("Index");
    }
}
