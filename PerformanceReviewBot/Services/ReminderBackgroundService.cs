namespace PerformanceReviewBot.Services;

public class ReminderBackgroundService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private Timer? _timer;

    public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reminder Background Service is starting.");

        // Run daily at 9 AM (simulated - runs every 24 hours from start)
        var dueTime = TimeSpan.Zero; // Start immediately for demo
        var period = TimeSpan.FromHours(24); // Run every 24 hours

        _timer = new Timer(DoWork, null, dueTime, period);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("Reminder Background Service is executing daily reminder check.");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<ReminderService>();
            await reminderService.ProcessCurrentMonthRemindersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing reminder background service.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reminder Background Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
