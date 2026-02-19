using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for peer feedback on performance reviews.
/// Maps routes under <c>/api/reviews/{reviewId}/feedback</c>.
/// </summary>
public static class FeedbackEndpoints
{
    /// <summary>
    /// Registers the feedback endpoint group on the application.
    /// </summary>
    public static void MapFeedbackEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reviews/{reviewId:int}/feedback")
            .WithTags("Feedback");

        group.MapGet("/", GetByReviewAsync);
        group.MapPost("/", SubmitAsync);
    }

    /// <summary>
    /// GET /api/reviews/{reviewId}/feedback — list all feedback for a review.
    /// </summary>
    private static async Task<IResult> GetByReviewAsync(
        int reviewId,
        FeedbackService service)
    {
        var feedbacks = await service.GetByReviewAsync(reviewId);

        return Results.Ok(feedbacks.Select(ToResponse));
    }

    /// <summary>
    /// POST /api/reviews/{reviewId}/feedback — submit peer feedback.
    /// </summary>
    private static async Task<IResult> SubmitAsync(
        int reviewId,
        FeedbackRequest request,
        FeedbackService service)
    {
        try
        {
            var feedback = await service.SubmitAsync(reviewId, request.AuthorId, request.Content);

            // Reload all feedback for this review (includes Author) to build the response.
            var feedbacks = await service.GetByReviewAsync(reviewId);
            var created = feedbacks.First(f => f.Id == feedback.Id);

            return Results.Created(
                $"/api/reviews/{reviewId}/feedback",
                ToResponse(created));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Maps a <see cref="Feedback"/> entity to a <see cref="FeedbackResponse"/> DTO.
    /// </summary>
    private static FeedbackResponse ToResponse(Feedback feedback) => new(
        feedback.Id,
        feedback.Author?.FullName ?? "Unknown",
        feedback.Content,
        feedback.SubmittedAt);
}
