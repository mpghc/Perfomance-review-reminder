using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for notification management.
/// Maps routes under <c>/api/notifications</c>.
/// </summary>
public static class NotificationEndpoints
{
    /// <summary>
    /// Registers the <c>/api/notifications</c> endpoint group on the application.
    /// </summary>
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapGet("/", GetByRecipientAsync);
        group.MapPatch("/{id:int}/read", MarkAsReadAsync);
    }

    /// <summary>
    /// GET /api/notifications?recipientId={id} — list notifications for a recipient.
    /// </summary>
    private static async Task<IResult> GetByRecipientAsync(
        NotificationService service,
        int? recipientId = null)
    {
        if (recipientId is null)
        {
            return Results.BadRequest(new { message = "Query parameter 'recipientId' is required." });
        }

        var notifications = await service.GetByRecipientAsync(recipientId.Value);

        return Results.Ok(notifications.Select(ToResponse));
    }

    /// <summary>
    /// PATCH /api/notifications/{id}/read — mark a notification as read.
    /// </summary>
    private static async Task<IResult> MarkAsReadAsync(
        int id,
        NotificationService service)
    {
        try
        {
            await service.MarkAsReadAsync(id);

            return Results.Ok(new { message = "Notification marked as read." });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Maps a <see cref="Notification"/> entity to a <see cref="NotificationResponse"/> DTO.
    /// </summary>
    private static NotificationResponse ToResponse(Notification notification) => new(
        notification.Id,
        notification.Message,
        notification.Type.ToString(),
        notification.IsRead,
        notification.CreatedAt);
}
