using PerformanceReviewBot.Models;

namespace PerformanceReviewBot.Services;

public interface IReminderService
{
    Task<List<PerformanceReview>> GetAllAsync();
    Task<PerformanceReview?> GetByIdAsync(int id);
    Task CreateAsync(PerformanceReview review);
    Task UpdateAsync(PerformanceReview review);
    Task DeleteAsync(int id);
    Task<List<PerformanceReview>> GetReviewsThisMonthAsync();
    Task<List<PerformanceReview>> GetOverdueReviewsAsync();
}
