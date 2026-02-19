using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Components;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Endpoints;
using PerformanceReviewReminderBot.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<TeammateService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ReminderService>();
builder.Services.AddHostedService<ReminderBackgroundService>();

var app = builder.Build();

// Apply pending migrations and seed demo data on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Enforce HSTS (HTTP Strict Transport Security) in production to prevent downgrade attacks.
    app.UseHsts();
}

// Redirect all HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Add security headers to every response to mitigate common web vulnerabilities:
// - X-Content-Type-Options: prevents MIME-type sniffing
// - X-Frame-Options: prevents clickjacking by disallowing iframe embedding
// - Referrer-Policy: limits referrer information sent to third-party sites
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseAntiforgery();

app.MapEmployeeEndpoints();
app.MapReviewEndpoints();
app.MapFeedbackEndpoints();
app.MapNotificationEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Make the implicit Program class public so WebApplicationFactory<Program> can access it.
public partial class Program { }
