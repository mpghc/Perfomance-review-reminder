# Iteration 7: Reminder Service & Background Worker

**Labels:** `iteration`, `services`, `background-service`, `critical`
**Priority:** Critical
**Estimated Time:** 2-3 sessions
**Depends on:** Iteration 3, Iteration 6

## Goal
Implement the reminder logic and background service for daily execution to check reviews and log reminders.

## Tasks

### Reminder Service
- [ ] Create IReminderService interface
- [ ] Implement ReminderService with methods:
  - `Task<List<PerformanceReview>> GetCurrentMonthReviewsAsync()`
  - `Task<List<int>> IdentifyMissingFeedbackAsync(int reviewId)`
  - `Task SendReminderAsync(int reviewId, string recipientType, string recipientName)`
  - `Task LogReminderAsync(int reviewId, string recipientType, string notes)`
  - `Task ProcessDailyRemindersAsync()` - Main entry point
- [ ] Implement reminder logic:
  - Identify reviews scheduled for current month
  - For each review, check if feedbacks are missing
  - If missing, log reminder for team members
  - If review approaching deadline (within 3 days), log reminder for manager
  - All logging is transactional

### Background Service
- [ ] Create ReminderBackgroundService inheriting from BackgroundService
  - Execute on startup and then daily
  - Configurable interval via appsettings.json
  - Call ReminderService.ProcessDailyRemindersAsync()
  - Handle exceptions gracefully
  - Log all activities
  - Support graceful shutdown

### Configuration
- [ ] Add ReminderSettings section to appsettings.json:
  ```json
  "ReminderSettings": {
    "Enabled": true,
    "IntervalHours": 24,
    "ManagerReminderDaysBeforeDeadline": 3,
    "RunOnStartup": true
  }
  ```
- [ ] Create ReminderSettings configuration class
- [ ] Bind configuration in Program.cs

### Service Registration
- [ ] Register ReminderService as Scoped in Program.cs
- [ ] Register ReminderBackgroundService as Hosted Service
- [ ] Configure ReminderSettings from appsettings.json

### Logging
- [ ] Log start/stop of background service
- [ ] Log each reminder check cycle
- [ ] Log number of reviews processed
- [ ] Log number of reminders sent
- [ ] Log any errors with full details
- [ ] Use structured logging with key properties

## Reminder Logic Flow

```
ProcessDailyRemindersAsync():
1. Get all reviews for current month (ScheduledDate.Month == DateTime.Now.Month)
2. For each review:
   a. Skip if status is Completed or Cancelled
   b. Get missing feedback provider IDs
   c. If any missing:
      - Log reminder for each missing provider (RecipientType: "Reviewer")
   d. If review date is within N days:
      - Log reminder for department manager (RecipientType: "Manager")
3. Save all reminder logs in one transaction
4. Return count of reminders logged
```

## Implementation Example

### IReminderService.cs
```csharp
public interface IReminderService
{
    Task<List<PerformanceReview>> GetCurrentMonthReviewsAsync();
    Task<List<int>> IdentifyMissingFeedbackAsync(int reviewId);
    Task LogReminderAsync(int reviewId, string recipientType, string notes);
    Task<int> ProcessDailyRemindersAsync();
}
```

### ReminderBackgroundService.cs
```csharp
public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly ReminderSettings _settings;

    public ReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReminderBackgroundService> logger,
        IOptions<ReminderSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder Background Service started");

        if (_settings.RunOnStartup)
        {
            await ProcessReminders(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(_settings.IntervalHours), stoppingToken);
            await ProcessReminders(stoppingToken);
        }

        _logger.LogInformation("Reminder Background Service stopped");
    }

    private async Task ProcessReminders(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            
            var count = await reminderService.ProcessDailyRemindersAsync();
            _logger.LogInformation("Processed reminders. Count: {Count}", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminders");
        }
    }
}
```

## Transactional Consistency
- All reminder logs for a single execution cycle must be created in one transaction
- Use single `SaveChangesAsync()` call after creating all reminder log entities
- If transaction fails, all reminder logs should be rolled back
- Log transaction failures for investigation

## Testing Strategy (for Iteration 9)
- Unit test GetCurrentMonthReviewsAsync with different months
- Unit test IdentifyMissingFeedbackAsync with various feedback scenarios
- Unit test ProcessDailyRemindersAsync with mocked data
- Mock DateTime.Now to test month boundary cases
- Verify transactionality of reminder logging
- Test background service startup and shutdown

## Acceptance Criteria
- [ ] ReminderService correctly identifies current month reviews
- [ ] Missing feedbacks are accurately identified
- [ ] Reminders are logged to ReminderLog table
- [ ] Background service runs on schedule
- [ ] Background service can be enabled/disabled via config
- [ ] All operations are transactional
- [ ] Comprehensive logging throughout process
- [ ] Graceful error handling
- [ ] Service can be started and stopped without errors
- [ ] No complex scheduling frameworks used
- [ ] Code follows .NET async patterns

## Manual Testing
```bash
# Run application
dotnet run

# Check logs for reminder service startup
# Wait for configured interval or trigger manually
# Verify ReminderLog table has entries
# Query: SELECT * FROM ReminderLogs ORDER BY SentAt DESC
```

## SQL Verification Query
```sql
-- Check reminder logs created today
SELECT rl.*, pr.ScheduledDate, e.Name as EmployeeName
FROM ReminderLogs rl
JOIN PerformanceReviews pr ON rl.ReviewId = pr.Id
JOIN Employees e ON pr.EmployeeId = e.Id
WHERE DATE(rl.SentAt) = DATE('now')
ORDER BY rl.SentAt DESC;
```

## Dependencies
- Iteration 3 must be completed (Services)
- Iteration 6 must be completed (Feedback logic)

## Notes
- Keep scheduling simple - just daily checks
- No external scheduling frameworks (no Quartz, Hangfire)
- Simulated sending - just log, don't actually send emails/notifications
- For testing, set IntervalHours to 0.01 (36 seconds) temporarily
- Human reviews reminder logic before implementation
- Consider time zones if application is used globally (future enhancement)
