using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Components;
using PerformanceReviewBot.Data;
using PerformanceReviewBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=performancereview.db"));

// Add application services
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<PerformanceReviewService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<ReminderService>();

// Add background service
builder.Services.AddHostedService<ReminderBackgroundService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
