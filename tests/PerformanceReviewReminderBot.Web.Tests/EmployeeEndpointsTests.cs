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
/// Integration tests for the <c>/api/employees</c> and <c>/api/employees/{id}/teammates</c>
/// Minimal API endpoints. Uses <see cref="WebApplicationFactory{TEntryPoint}"/> with an
/// in-memory SQLite database to isolate each test.
/// </summary>
public class EmployeeEndpointsTests : IClassFixture<EmployeeEndpointsTests.TestFactory>
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;

    public EmployeeEndpointsTests(TestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSeededEmployees()
    {
        var response = await _client.GetAsync("/api/employees");

        response.EnsureSuccessStatusCode();
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeResponse>>();
        Assert.NotNull(employees);
        Assert.True(employees.Count >= 5, "Seed data should include at least 5 employees.");
    }

    [Fact]
    public async Task GetAll_WithRoleFilter_ReturnsOnlyMatching()
    {
        var response = await _client.GetAsync("/api/employees?role=TalentManager");

        response.EnsureSuccessStatusCode();
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeResponse>>();
        Assert.NotNull(employees);
        Assert.All(employees, e => Assert.Equal("TalentManager", e.Role));
    }

    [Fact]
    public async Task GetById_ExistingEmployee_ReturnsOk()
    {
        // Seed data always has employee Id=1
        var response = await _client.GetAsync("/api/employees/1");

        response.EnsureSuccessStatusCode();
        var employee = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(employee);
        Assert.Equal(1, employee.Id);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/employees/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidEmployee_Returns201()
    {
        var request = new EmployeeRequest("New Person", "new@test.com", (int)EmployeeRole.Employee, null);

        var response = await _client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(created);
        Assert.Equal("New Person", created.FullName);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task Create_MissingName_ReturnsBadRequest()
    {
        var request = new EmployeeRequest("", "test@test.com", (int)EmployeeRole.Employee, null);

        var response = await _client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingEmployee_ReturnsOk()
    {
        // Create an employee to update
        var createRequest = new EmployeeRequest("Update Me", "update@test.com", (int)EmployeeRole.Employee, null);
        var createResponse = await _client.PostAsJsonAsync("/api/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>();

        var updateRequest = new EmployeeRequest("Updated Name", "updated@test.com", (int)EmployeeRole.Employee, null);

        var response = await _client.PutAsJsonAsync($"/api/employees/{created!.Id}", updateRequest);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.FullName);
    }

    [Fact]
    public async Task Delete_EmployeeWithoutReviews_ReturnsNoContent()
    {
        // Create a disposable employee
        var request = new EmployeeRequest("Delete Me", "delete@test.com", (int)EmployeeRole.Employee, null);
        var createResponse = await _client.PostAsJsonAsync("/api/employees", request);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>();

        var response = await _client.DeleteAsync($"/api/employees/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_EmployeeWithReviews_ReturnsConflict()
    {
        // Seeded employee Tom (Id=2) has a performance review
        var response = await _client.DeleteAsync("/api/employees/2");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetTeammates_ReturnsSeededTeammates()
    {
        // Seeded employee Tom (Id=2) has teammates: Alice(3), Bob(4), Carol(5)
        var response = await _client.GetAsync("/api/employees/2/teammates");

        response.EnsureSuccessStatusCode();
        var teammates = await response.Content.ReadFromJsonAsync<List<TeammateResponse>>();
        Assert.NotNull(teammates);
        Assert.Equal(3, teammates.Count);
    }

    [Fact]
    public async Task AddAndRemoveTeammate_HappyPath()
    {
        // Create two fresh employees to avoid seed interference
        var emp1 = new EmployeeRequest("TP1", "tp1@test.com", (int)EmployeeRole.Employee, null);
        var emp2 = new EmployeeRequest("TP2", "tp2@test.com", (int)EmployeeRole.Employee, null);
        var r1 = await (await _client.PostAsJsonAsync("/api/employees", emp1))
            .Content.ReadFromJsonAsync<EmployeeResponse>();
        var r2 = await (await _client.PostAsJsonAsync("/api/employees", emp2))
            .Content.ReadFromJsonAsync<EmployeeResponse>();

        // Add teammate
        var addResponse = await _client.PostAsync(
            $"/api/employees/{r1!.Id}/teammates/{r2!.Id}", null);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);

        // Verify bidirectional
        var teammates1 = await (await _client.GetAsync($"/api/employees/{r1.Id}/teammates"))
            .Content.ReadFromJsonAsync<List<TeammateResponse>>();
        var teammates2 = await (await _client.GetAsync($"/api/employees/{r2.Id}/teammates"))
            .Content.ReadFromJsonAsync<List<TeammateResponse>>();
        Assert.Contains(teammates1!, t => t.Id == r2.Id);
        Assert.Contains(teammates2!, t => t.Id == r1.Id);

        // Remove teammate
        var removeResponse = await _client.DeleteAsync(
            $"/api/employees/{r1.Id}/teammates/{r2.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        // Verify removed
        var after = await (await _client.GetAsync($"/api/employees/{r1.Id}/teammates"))
            .Content.ReadFromJsonAsync<List<TeammateResponse>>();
        Assert.DoesNotContain(after!, t => t.Id == r2.Id);
    }

    /// <summary>
    /// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> that replaces the
    /// production SQLite database with a shared in-memory SQLite connection and
    /// pre-seeds the data for every test run.
    /// </summary>
    public class TestFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public TestFactory()
        {
            // Keep the connection open so the in-memory database persists
            // across all requests and scoped DbContext instances.
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the production DbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // Use the shared in-memory connection for all DbContext instances.
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
