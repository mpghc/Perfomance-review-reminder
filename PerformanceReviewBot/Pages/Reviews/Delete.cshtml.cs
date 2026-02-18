using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Reviews;

public class DeleteModel : PageModel
{
    private readonly IReminderService _reminderService;

    public DeleteModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [BindProperty]
    public PerformanceReview Review { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await _reminderService.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        Review = review;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _reminderService.DeleteAsync(Review.Id);
        return RedirectToPage("Index");
    }
}
