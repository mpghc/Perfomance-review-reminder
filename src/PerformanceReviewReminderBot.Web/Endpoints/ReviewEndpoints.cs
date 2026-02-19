using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for performance review management.
/// Maps all routes under <c>/api/reviews</c>.
/// </summary>
public static class ReviewEndpoints
{
    /// <summary>
    /// Registers the <c>/api/reviews</c> endpoint group on the application.
    /// </summary>
    public static void MapReviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reviews")
            .WithTags("Reviews");

        group.MapGet("/", GetByManagerAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", ScheduleAsync);
        group.MapPatch("/{id:int}/status", UpdateStatusAsync);
    }

    /// <summary>
    /// GET /api/reviews?managerId={id} — list reviews for a TM's employees.
    /// </summary>
    private static async Task<IResult> GetByManagerAsync(
        ReviewService service,
        int? managerId = null)
    {
        if (managerId is null)
        {
            return Results.BadRequest(new { message = "Query parameter 'managerId' is required." });
        }

        var reviews = await service.GetByManagerAsync(managerId.Value);

        return Results.Ok(reviews.Select(ToResponse));
    }

    /// <summary>
    /// GET /api/reviews/{id} — get a single review with feedback progress.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(
        int id,
        ReviewService service)
    {
        var review = await service.GetByIdAsync(id);

        if (review is null)
        {
            return Results.NotFound(new { message = $"Review with Id {id} not found." });
        }

        return Results.Ok(ToResponse(review));
    }

    /// <summary>
    /// POST /api/reviews — schedule a new performance review.
    /// </summary>
    private static async Task<IResult> ScheduleAsync(
        ReviewRequest request,
        ReviewService service)
    {
        try
        {
            if (!DateOnly.TryParse(request.ReviewDate, out var reviewDate))
            {
                return Results.BadRequest(new { message = "Invalid ReviewDate format. Use yyyy-MM-dd." });
            }

            var review = await service.ScheduleAsync(request.EmployeeId, reviewDate);

            // Reload with includes for the response DTO.
            var created = await service.GetByIdAsync(review.Id);

            return Results.Created($"/api/reviews/{review.Id}", ToResponse(created!));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PATCH /api/reviews/{id}/status — update the status of an existing review.
    /// </summary>
    private static async Task<IResult> UpdateStatusAsync(
        int id,
        StatusUpdateRequest request,
        ReviewService service)
    {
        try
        {
            if (!Enum.TryParse<ReviewStatus>(request.Status, ignoreCase: true, out var status))
            {
                return Results.BadRequest(new { message = $"Invalid status '{request.Status}'. Use Scheduled, InProgress, or Completed." });
            }

            await service.UpdateStatusAsync(id, status);

            var updated = await service.GetByIdAsync(id);

            return Results.Ok(ToResponse(updated!));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Maps a <see cref="PerformanceReview"/> entity to a <see cref="ReviewResponse"/> DTO.
    /// </summary>
    private static ReviewResponse ToResponse(PerformanceReview review) => new(
        review.Id,
        review.Employee?.FullName ?? "Unknown",
        review.ReviewDate.ToString("yyyy-MM-dd"),
        review.Status.ToString(),
        review.Feedbacks.Count,
        review.Employee?.EmployeeTeammates.Count ?? 0);
}
