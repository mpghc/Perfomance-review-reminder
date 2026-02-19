using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Endpoints;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Tests;

/// <summary>
/// Integration tests verifying the security hardening changes:
/// input validation on request DTOs, unsafe enum guard, and security response headers.
/// </summary>
public class SecurityTests : IClassFixture<SecurityTests.TestFactory>
{
    private readonly HttpClient _client;

    public SecurityTests(TestFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Input Validation – EmployeeRequest ───────────────────

    [Fact]
    public async Task CreateEmployee_InvalidEmail_ReturnsBadRequest()
    {
        var request = new EmployeeRequest("Valid Name", "not-an-email", (int)EmployeeRole.Employee, null);

        var response = await _client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_NameExceedsMaxLength_ReturnsBadRequest()
    {
        var longName = new string('A', 201);
        var request = new EmployeeRequest(longName, "valid@test.com", (int)EmployeeRole.Employee, null);

        var response = await _client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_EmailExceedsMaxLength_ReturnsBadRequest()
    {
        var longEmail = new string('a', 192) + "@test.com"; // > 200 chars
        var request = new EmployeeRequest("Valid Name", longEmail, (int)EmployeeRole.Employee, null);

        var response = await _client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_InvalidRoleValue_ReturnsBadRequest()
    {
        // Role 999 is not a defined EmployeeRole value.
        var request = new EmployeeRequest("Valid Name", "valid@test.com", 999, null);

        var response = await _client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_InvalidRoleValue_ReturnsBadRequest()
    {
        // Role -1 is not a defined EmployeeRole value.
        var request = new EmployeeRequest("Valid Name", "valid@test.com", -1, null);

        var response = await _client.PutAsJsonAsync("/api/employees/1", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Input Validation – FeedbackRequest ───────────────────

    [Fact]
    public async Task SubmitFeedback_ContentExceedsMaxLength_ReturnsBadRequest()
    {
        var longContent = new string('x', 4001);
        var request = new FeedbackRequest(3, longContent);

        var response = await _client.PostAsJsonAsync("/api/reviews/1/feedback", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Security Response Headers ────────────────────────────

    [Fact]
    public async Task AnyResponse_HasXContentTypeOptionsHeader()
    {
        var response = await _client.GetAsync("/api/employees");

        Assert.True(
            response.Headers.TryGetValues("X-Content-Type-Options", out var values),
            "X-Content-Type-Options header should be present.");
        Assert.Contains("nosniff", values);
    }

    [Fact]
    public async Task AnyResponse_HasXFrameOptionsHeader()
    {
        var response = await _client.GetAsync("/api/employees");

        Assert.True(
            response.Headers.TryGetValues("X-Frame-Options", out var values),
            "X-Frame-Options header should be present.");
        Assert.Contains("DENY", values);
    }

    [Fact]
    public async Task AnyResponse_HasReferrerPolicyHeader()
    {
        var response = await _client.GetAsync("/api/employees");

        Assert.True(
            response.Headers.TryGetValues("Referrer-Policy", out var values),
            "Referrer-Policy header should be present.");
        Assert.Contains("strict-origin-when-cross-origin", values);
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
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}
