# Stage 7: Reminder Flow

## Objective
Implement the complete end-to-end reminder logic with background service execution and transactional persistence.

## Description
Build the reminder system that checks for performance reviews scheduled in the current month, identifies reviews with missing feedback, sends simulated reminders (logged instead of actual emails), and persists all reminder logs to the database. The system will run automatically via a background service on a daily schedule (simulated with shorter intervals for demonstration).

## Reminder Flow Steps

### 1. Check Current Month Reviews
**Logic:**
- Query `PerformanceReview` table for reviews where `ScheduledDate` is in the current month
- Filter by status: "Pending" or "InProgress" (exclude "Completed" and "Cancelled")
- Include related entities: Employee (reviewee), Reviewer, Feedback

**Implementation:**
```csharp
var currentMonth = DateTime.UtcNow.Month;
var currentYear = DateTime.UtcNow.Year;

var reviews = await context.PerformanceReviews
    .Include(r => r.Employee)
    .Include(r => r.Reviewer)
    .Include(r => r.Feedbacks)
    .Where(r => r.ScheduledDate.Month == currentMonth 
             && r.ScheduledDate.Year == currentYear
             && (r.Status == "Pending" || r.Status == "InProgress"))
    .ToListAsync();
```

### 2. Identify Missing Feedback
**Logic:**
- For each review, determine expected feedback providers (could be peers, managers, etc.)
- Compare expected providers with actual feedback submissions
- Identify employees who haven't submitted feedback yet
- Consider reviews that are past their scheduled date

**Considerations:**
- Who should provide feedback? (Design decision: all active employees, specific team members, or configured list)
- Has provider already submitted feedback? Check `Feedback.ProviderId`
- Has a reminder already been sent recently? Check `ReminderLog` within last 24-48 hours

### 3. Send Reminders (Simulated)
**Logic:**
- For each identified missing feedback case:
  - Create a reminder message (e.g., "Reminder: Please provide feedback for [Employee]'s review scheduled on [Date]")
  - Log the reminder instead of sending actual email
  - Include: ReviewId, RecipientId, ReminderType, Message, SentDate

**Simulation:**
- Log to application logger
- Persist to `ReminderLog` table
- Do NOT send actual emails (this is a simulation)

**Example Log Message:**
```
"Reminder sent to [RecipientName] ([RecipientEmail]) for review of [EmployeeName] scheduled on [Date]"
```

### 4. Persist Reminder Logs
**Logic:**
- Create `ReminderLog` entity for each reminder sent
- Use database transaction to ensure atomicity
- If any step fails, rollback entire batch

**Transactional Implementation:**
```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // Create reminder logs
    foreach (var reminder in remindersToSend)
    {
        context.ReminderLogs.Add(reminder);
    }
    
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
    
    logger.LogInformation($"Successfully sent {remindersToSend.Count} reminders");
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    logger.LogError(ex, "Failed to send reminders");
    throw;
}
```

### 5. Background Service Execution
**ReminderBackgroundService:**
- Inherits from `BackgroundService`
- Runs continuously in the background
- Executes reminder logic on a schedule (daily in production, shorter for demo)
- Uses scoped service provider to resolve `IReminderService`

**Implementation:**
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            
            try
            {
                await reminderService.SendRemindersAsync();
                logger.LogInformation("Reminder service executed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing reminder service");
            }
        }
        
        // Wait 24 hours (or shorter interval for demo)
        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
    }
}
```

## Configuration Options

### appsettings.json
```json
{
  "ReminderSettings": {
    "Enabled": true,
    "IntervalHours": 24,
    "MinimumHoursBetweenReminders": 48,
    "ReminderTypes": ["MissingFeedback", "UpcomingReview"]
  }
}
```

## Tasks
- [ ] Implement `GetReviewsNeedingRemindersAsync()` in `ReminderService`
  - Query current month reviews
  - Filter by status
  - Include related entities
- [ ] Implement missing feedback detection logic
  - Identify expected feedback providers
  - Compare with actual submissions
  - Check for recent reminders (avoid duplicates)
- [ ] Implement `LogReminderAsync()` in `ReminderService`
  - Create `ReminderLog` entity
  - Persist to database
- [ ] Implement `SendRemindersAsync()` in `ReminderService`
  - Orchestrate the complete flow
  - Use transaction for consistency
  - Log to application logger
  - Handle errors gracefully
- [ ] Implement `ReminderBackgroundService.ExecuteAsync()`
  - Set up continuous loop with delay
  - Create scoped service provider
  - Resolve `IReminderService`
  - Call `SendRemindersAsync()`
  - Handle cancellation token
- [ ] Configure reminder settings in `appsettings.json`
- [ ] Register `ReminderBackgroundService` as hosted service in `Program.cs`
- [ ] Add comprehensive logging throughout the flow
- [ ] Test reminder logic with sample data
- [ ] Test transactional behavior (rollback on failure)
- [ ] Test background service startup and execution

## Acceptance Criteria
- Reminder service correctly identifies reviews in current month
- Missing feedback detection works accurately
- Reminders are logged (not actually sent)
- All reminders are persisted to `ReminderLog` table
- Transactional consistency is maintained (all-or-nothing)
- Background service starts with application
- Background service executes on schedule
- Duplicate reminders are prevented (check recent reminders)
- Errors are handled gracefully without crashing the service
- Comprehensive logging for troubleshooting
- Service respects cancellation token for graceful shutdown
- Configuration options are read from appsettings.json

## Technical Notes
- Use UTC for all date/time operations
- Consider time zones if needed (probably not for demo)
- Use `IServiceScopeFactory` in background service for scoped dependencies
- Implement idempotency where possible
- Log at appropriate levels (Info for success, Error for failures, Debug for details)
- Test with different scenarios:
  - No reviews in current month
  - All feedback already submitted
  - Some feedback missing
  - Transaction failure

## Error Handling
- Catch and log exceptions at service level
- Don't let background service crash on errors
- Retry logic (optional, but nice to have)
- Alert/notification on repeated failures (optional)

## Testing Considerations
- Create unit tests with in-memory database
- Mock date/time for testing specific months
- Test transaction rollback scenarios
- Test background service cancellation

## Dependencies
- Stage 1 (Solution Structure) must be complete
- Stage 2 (Folder Organization) must be complete
- Stage 3 (Entities and Relationships) must be complete
- Stage 4 (Services Definition) must be complete

## Estimated Effort
Medium-Large - 4-5 hours
