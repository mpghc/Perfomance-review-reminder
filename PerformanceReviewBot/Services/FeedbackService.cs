using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data;
using PerformanceReviewBot.Data.Entities;

namespace PerformanceReviewBot.Services;

public class FeedbackService
{
    private readonly AppDbContext _context;

    public FeedbackService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Feedback>> GetFeedbackByReviewIdAsync(int reviewId)
    {
        return await _context.Feedbacks
            .Include(f => f.Reviewer)
            .Where(f => f.PerformanceReviewId == reviewId)
            .OrderByDescending(f => f.SubmittedDate)
            .ToListAsync();
    }

    public async Task<List<Feedback>> GetFeedbackByReviewerIdAsync(int reviewerId)
    {
        return await _context.Feedbacks
            .Include(f => f.PerformanceReview)
            .ThenInclude(pr => pr.Employee)
            .Where(f => f.ReviewerId == reviewerId)
            .OrderByDescending(f => f.SubmittedDate)
            .ToListAsync();
    }

    public async Task<Feedback> SubmitFeedbackAsync(Feedback feedback)
    {
        // Check if feedback already exists
        var existing = await HasSubmittedFeedbackAsync(feedback.PerformanceReviewId, feedback.ReviewerId);
        if (existing)
        {
            throw new InvalidOperationException("Feedback already submitted for this review by this reviewer.");
        }

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        return feedback;
    }

    public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync();
        return feedback;
    }

    public async Task<bool> HasSubmittedFeedbackAsync(int reviewId, int reviewerId)
    {
        return await _context.Feedbacks
            .AnyAsync(f => f.PerformanceReviewId == reviewId && f.ReviewerId == reviewerId);
    }
}
