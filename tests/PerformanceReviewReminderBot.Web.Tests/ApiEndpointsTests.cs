using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Endpoints;

namespace PerformanceReviewReminderBot.Web.Tests;

/// <summary>
/// Integration tests for the <c>/api/reviews</c>, <c>/api/reviews/{id}/feedback</c>,
/// and <c>/api/notifications</c> Minimal API endpoints.
/// Uses <see cref="WebApplicationFactory{TEntryPoint}"/> with an in-memory SQLite database.
/// </summary>
public class ApiEndpointsTests : IClassFixture<ApiEndpointsTests.TestFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointsTests(TestFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Review Endpoints ─────────────────────────────────────

    [Fact]
    public async Task GetReviews_WithManagerId_ReturnsSeededReview()
    {
        // Bill (Id=1) manages Tom who has a seeded review.
        var response = await _client.GetAsync("/api/reviews?managerId=1");

        response.EnsureSuccessStatusCode();
        var reviews = await response.Content.ReadFromJsonAsync<List<ReviewResponse>>();
        Assert.NotNull(reviews);
        Assert.Contains(reviews, r => r.EmployeeName == "Tom");
    }

    [Fact]
    public async Task GetReviews_WithoutManagerId_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/reviews");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetReviewById_Existing_ReturnsOk()
    {
        // Seeded review Id=1
        var response = await _client.GetAsync("/api/reviews/1");

        response.EnsureSuccessStatusCode();
        var review = await response.Content.ReadFromJsonAsync<ReviewResponse>();
        Assert.NotNull(review);
        Assert.Equal(1, review.Id);
        Assert.Equal("Tom", review.EmployeeName);
        Assert.False(string.IsNullOrEmpty(review.Status));
    }

    [Fact]
    public async Task GetReviewById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/reviews/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ScheduleReview_ValidData_Returns201()
    {
        // Alice (Id=3) is managed by Bill and has teammates.
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)).ToString("yyyy-MM-dd");
        var request = new ReviewRequest(3, futureDate);

        var response = await _client.PostAsJsonAsync("/api/reviews", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ReviewResponse>();
        Assert.NotNull(created);
        Assert.Equal("Alice", created.EmployeeName);
        Assert.Equal("Scheduled", created.Status);
    }

    [Fact]
    public async Task ScheduleReview_PastDate_ReturnsBadRequest()
    {
        var request = new ReviewRequest(3, "2020-01-01");

        var response = await _client.PostAsJsonAsync("/api/reviews", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ValidStatus_ReturnsOk()
    {
        var request = new StatusUpdateRequest("InProgress");

        var response = await _client.PatchAsJsonAsync("/api/reviews/1/status", request);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<ReviewResponse>();
        Assert.NotNull(updated);
        Assert.Equal("InProgress", updated.Status);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatus_ReturnsBadRequest()
    {
        var request = new StatusUpdateRequest("Invalid");

        var response = await _client.PatchAsJsonAsync("/api/reviews/1/status", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Feedback Endpoints ───────────────────────────────────

    [Fact]
    public async Task GetFeedback_EmptyReview_ReturnsEmptyList()
    {
        // Review Id=1 has no feedback yet.
        var response = await _client.GetAsync("/api/reviews/1/feedback");

        response.EnsureSuccessStatusCode();
        var feedbacks = await response.Content.ReadFromJsonAsync<List<FeedbackResponse>>();
        Assert.NotNull(feedbacks);
        Assert.Empty(feedbacks);
    }

    [Fact]
    public async Task SubmitAndGetFeedback_HappyPath()
    {
        // First schedule a fresh review so we don't interfere with other tests.
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(60)).ToString("yyyy-MM-dd");
        var scheduleResponse = await _client.PostAsJsonAsync("/api/reviews", new ReviewRequest(2, futureDate));
        scheduleResponse.EnsureSuccessStatusCode();
        var review = await scheduleResponse.Content.ReadFromJsonAsync<ReviewResponse>();

        // Alice (Id=3) is a teammate of Tom (Id=2).
        var feedbackRequest = new FeedbackRequest(3, "Great collaboration!");
        var submitResponse = await _client.PostAsJsonAsync($"/api/reviews/{review!.Id}/feedback", feedbackRequest);

        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);
        var feedback = await submitResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(feedback);
        Assert.Equal("Alice", feedback.AuthorName);
        Assert.Equal("Great collaboration!", feedback.Content);

        // Verify it appears in GET.
        var listResponse = await _client.GetAsync($"/api/reviews/{review.Id}/feedback");
        listResponse.EnsureSuccessStatusCode();
        var feedbacks = await listResponse.Content.ReadFromJsonAsync<List<FeedbackResponse>>();
        Assert.NotNull(feedbacks);
        Assert.Single(feedbacks);
    }

    [Fact]
    public async Task SubmitFeedback_NotTeammate_ReturnsBadRequest()
    {
        // Bill (Id=1) is not a teammate of Tom (Id=2).
        var request = new FeedbackRequest(1, "Feedback from non-teammate");
        var response = await _client.PostAsJsonAsync("/api/reviews/1/feedback", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Notification Endpoints ───────────────────────────────

    [Fact]
    public async Task GetNotifications_WithRecipientId_ReturnsSeeded()
    {
        // Alice (Id=3) has a seeded notification.
        var response = await _client.GetAsync("/api/notifications?recipientId=3");

        response.EnsureSuccessStatusCode();
        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        Assert.NotNull(notifications);
        Assert.NotEmpty(notifications);
        Assert.All(notifications, n => Assert.Equal("Reminder", n.Type));
    }

    [Fact]
    public async Task GetNotifications_WithoutRecipientId_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_ExistingNotification_ReturnsOk()
    {
        // Get Alice's notifications to find an Id.
        var listResponse = await _client.GetAsync("/api/notifications?recipientId=3");
        var notifications = await listResponse.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        var notificationId = notifications!.First().Id;

        var response = await _client.PatchAsync($"/api/notifications/{notificationId}/read", null);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task MarkAsRead_NonExistent_ReturnsNotFound()
    {
        var response = await _client.PatchAsync("/api/notifications/9999/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> with in-memory SQLite
    /// for isolated integration testing.
    /// </summary>
    public class TestFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public TestFactory()
        {
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Dispose();
        }
    }
}
