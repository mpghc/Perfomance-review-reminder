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
}


app.UseAntiforgery();

app.MapEmployeeEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Make the implicit Program class public so WebApplicationFactory<Program> can access it.
public partial class Program { }
