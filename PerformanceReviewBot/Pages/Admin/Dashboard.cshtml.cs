using Microsoft.AspNetCore.Mvc.RazorPages;
using PerformanceReviewBot.Models;
using PerformanceReviewBot.Services;

namespace PerformanceReviewBot.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IReminderService _reminderService;

    public DashboardModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    public List<PerformanceReview> OverdueReviews { get; set; } = new();
    public List<PerformanceReview> ThisMonthReviews { get; set; } = new();

    public async Task OnGetAsync()
    {
        OverdueReviews = await _reminderService.GetOverdueReviewsAsync();
        ThisMonthReviews = await _reminderService.GetReviewsThisMonthAsync();
    }
}
