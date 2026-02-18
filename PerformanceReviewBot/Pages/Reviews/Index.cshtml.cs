using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly IReminderService _reminderService;

    public IndexModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    public List<PerformanceReview> Reviews { get; set; } = new();

    public async Task OnGetAsync()
    {
        Reviews = await _reminderService.GetAllAsync();
    }
}
