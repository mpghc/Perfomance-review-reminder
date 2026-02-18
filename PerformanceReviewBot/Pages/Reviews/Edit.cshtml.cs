using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Reviews;

public class EditModel : PageModel
{
    private readonly IReminderService _reminderService;
    private readonly IEmployeeService _employeeService;

    public EditModel(IReminderService reminderService, IEmployeeService employeeService)
    {
        _reminderService = reminderService;
        _employeeService = employeeService;
    }

    [BindProperty]
    public PerformanceReview Review { get; set; } = new();

    public SelectList EmployeeList { get; set; } = null!;
    public SelectList StatusList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await _reminderService.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        Review = review;
        await PopulateDropdowns();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return Page();
        }

        await _reminderService.UpdateAsync(Review);
        return RedirectToPage("Index");
    }

    private async Task PopulateDropdowns()
    {
        var employees = await _employeeService.GetAllAsync();
        EmployeeList = new SelectList(employees, "Id", "FullName");
        StatusList = new SelectList(ReviewStatus.All);
    }
}
