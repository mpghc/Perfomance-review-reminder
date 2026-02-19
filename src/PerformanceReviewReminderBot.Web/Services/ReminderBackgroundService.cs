namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Background service that periodically runs <see cref="ReminderService.ProcessAsync"/>
/// to generate reminder and overdue notifications.
/// This is a thin wrapper — all logic lives in <see cref="ReminderService"/>.
/// </summary>
/// <remarks>
/// <para>
/// This service is a <b>singleton</b>, but <see cref="ReminderService"/> and
/// <see cref="Data.AppDbContext"/> are <b>scoped</b>. A new
/// <see cref="IServiceScope"/> is created on every tick to resolve them safely.
/// </para>
/// <para>
/// The first run triggers immediately on startup (for demo), then repeats
/// at the interval configured in <c>Reminders:IntervalMinutes</c> (default 1440 = 24 h).
/// </para>
/// </remarks>
public class ReminderBackgroundService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<ReminderBackgroundService> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ReminderBackgroundService> _logger = logger;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderBackgroundService started.");

        // Run immediately on startup for demo purposes.
        await RunReminderProcessAsync();

        var intervalMinutes = _configuration.GetValue("Reminders:IntervalMinutes", 1440);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        _logger.LogInformation(
            "ReminderBackgroundService will repeat every {Interval} minutes.", intervalMinutes);

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunReminderProcessAsync();
        }
    }

    /// <summary>
    /// Creates a scope, resolves <see cref="ReminderService"/>, and calls
    /// <see cref="ReminderService.ProcessAsync"/>. All exceptions are caught
    /// and logged to prevent the background service from crashing.
    /// </summary>
    private async Task RunReminderProcessAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<ReminderService>();

            var created = await reminderService.ProcessAsync(DateTime.UtcNow);

            _logger.LogInformation(
                "ReminderBackgroundService tick completed: {Created} notification(s) created.", created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReminderBackgroundService encountered an error during processing.");
        }
    }
}
