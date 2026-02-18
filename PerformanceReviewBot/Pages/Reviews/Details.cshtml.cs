using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Reviews;

public class DetailsModel : PageModel
{
    private readonly IReminderService _reminderService;

    public DetailsModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

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
}
