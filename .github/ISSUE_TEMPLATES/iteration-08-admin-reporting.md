# Iteration 8: Admin Reporting & Dashboard

**Labels:** `iteration`, `ui`, `admin`, `reporting`
**Priority:** High
**Estimated Time:** 2 sessions
**Depends on:** Iteration 4, Iteration 7

## Goal
Create admin pages for managers to view reports, dashboard, and missing feedback information.

## Tasks

### Report Service
- [ ] Create IReportService interface
- [ ] Implement ReportService with methods:
  - `Task<DashboardSummary> GetDashboardSummaryAsync()`
  - `Task<List<MissingFeedbackReportItem>> GetMissingFeedbackReportAsync()`
  - `Task<List<ReminderLogReportItem>> GetReminderLogReportAsync(DateTime? from, DateTime? to)`
  - `Task<List<ReviewStatusSummary>> GetReviewStatusSummaryAsync()`

### DTOs for Reports
- [ ] Create DashboardSummary DTO
  - TotalReviews, PendingReviews, CompletedReviews
  - TotalMissingFeedbacks
  - RemindersLastWeek
  - UpcomingReviewsCount
- [ ] Create MissingFeedbackReportItem DTO
  - EmployeeName, ReviewDate, DepartmentName
  - ExpectedFeedbackCount, SubmittedFeedbackCount, MissingCount
  - ReviewerNames (who haven't submitted)
- [ ] Create ReminderLogReportItem DTO
  - ReviewId, EmployeeName, RecipientType
  - SentAt, Notes
- [ ] Create ReviewStatusSummary DTO
  - StatusName, Count, Percentage

### Admin Pages
- [ ] Create AdminDashboard.razor (@layout AdminLayout)
  - Summary cards showing key metrics
  - Quick statistics (total reviews, pending, completed)
  - Missing feedbacks count with alert badge
  - Recent reminders list (last 10)
  - Quick action buttons to reports
  - Charts (optional, simple bar/pie charts using Chart.js or similar)
- [ ] Create MissingFeedbackReport.razor (@layout AdminLayout)
  - Table of reviews with missing feedbacks
  - Columns: Employee, Review Date, Department, Expected, Submitted, Missing, Reviewers
  - Filter by: Department, Date Range
  - Sort by: Date, Missing Count
  - Export to CSV button (optional)
  - Color-coded rows (red for urgent, yellow for approaching)
- [ ] Create ReminderLogReport.razor (@layout AdminLayout)
  - Table showing all reminder logs
  - Columns: Review, Employee, Recipient Type, Sent Date, Notes
  - Filter by: Date Range, Recipient Type
  - Pagination
  - Search functionality

### Admin Navigation
- [ ] Update AdminNavMenu.razor with links:
  - Dashboard (default)
  - Missing Feedback Report
  - Reminder Log Report
  - Review Status Summary
  - Back to User View

### Service Registration
- [ ] Register ReportService in Program.cs as Scoped

## Page Layout Example

### AdminDashboard.razor
```razor
@page "/admin"
@page "/admin/dashboard"
@layout AdminLayout
@inject IReportService ReportService

<h3>Admin Dashboard</h3>

@if (summary != null)
{
    <div class="row">
        <div class="col-md-3">
            <div class="card text-white bg-primary mb-3">
                <div class="card-body">
                    <h5 class="card-title">Total Reviews</h5>
                    <p class="card-text display-4">@summary.TotalReviews</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-white bg-warning mb-3">
                <div class="card-body">
                    <h5 class="card-title">Pending Reviews</h5>
                    <p class="card-text display-4">@summary.PendingReviews</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-white bg-success mb-3">
                <div class="card-body">
                    <h5 class="card-title">Completed Reviews</h5>
                    <p class="card-text display-4">@summary.CompletedReviews</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-white bg-danger mb-3">
                <div class="card-body">
                    <h5 class="card-title">Missing Feedbacks</h5>
                    <p class="card-text display-4">@summary.TotalMissingFeedbacks</p>
                </div>
            </div>
        </div>
    </div>

    <div class="row mt-4">
        <div class="col-md-12">
            <h5>Quick Actions</h5>
            <a href="/admin/missing-feedback" class="btn btn-primary">View Missing Feedback Report</a>
            <a href="/admin/reminder-logs" class="btn btn-secondary">View Reminder Logs</a>
        </div>
    </div>
}
```

### MissingFeedbackReport.razor
```razor
@page "/admin/missing-feedback"
@layout AdminLayout
@inject IReportService ReportService

<h3>Missing Feedback Report</h3>

<div class="mb-3">
    <label>Filter by Department:</label>
    <select class="form-select" @bind="selectedDepartment">
        <option value="">All Departments</option>
        @foreach (var dept in departments)
        {
            <option value="@dept">@dept</option>
        }
    </select>
</div>

<table class="table table-striped">
    <thead>
        <tr>
            <th>Employee</th>
            <th>Review Date</th>
            <th>Department</th>
            <th>Expected</th>
            <th>Submitted</th>
            <th>Missing</th>
            <th>Pending Reviewers</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in reportItems)
        {
            <tr class="@GetRowClass(item)">
                <td>@item.EmployeeName</td>
                <td>@item.ReviewDate.ToShortDateString()</td>
                <td>@item.DepartmentName</td>
                <td>@item.ExpectedFeedbackCount</td>
                <td>@item.SubmittedFeedbackCount</td>
                <td><span class="badge bg-danger">@item.MissingCount</span></td>
                <td>@string.Join(", ", item.ReviewerNames)</td>
            </tr>
        }
    </tbody>
</table>

@code {
    private string GetRowClass(MissingFeedbackReportItem item)
    {
        if (item.ReviewDate < DateTime.Now.AddDays(3))
            return "table-danger"; // Urgent
        else if (item.ReviewDate < DateTime.Now.AddDays(7))
            return "table-warning"; // Approaching
        return "";
    }
}
```

## Query Optimization
- Use efficient EF Core queries with proper includes
- Avoid N+1 queries (use Include() and ThenInclude())
- Consider using projection (Select) to return only needed data
- Add indexes on frequently queried columns:
  - PerformanceReviews.ScheduledDate
  - ReminderLogs.SentAt
  - Feedbacks.ReviewId

## Acceptance Criteria
- [ ] Admin dashboard displays correct summary statistics
- [ ] Missing feedback report shows accurate data
- [ ] Report includes all reviews with missing feedbacks
- [ ] Reminder log report displays all reminders
- [ ] Filters work correctly on all reports
- [ ] Admin layout clearly distinguishes from user layout
- [ ] All admin pages use AdminLayout
- [ ] Navigation between admin pages works
- [ ] Queries are efficient (no N+1 problems)
- [ ] Reports load quickly (< 2 seconds for reasonable data)
- [ ] Color-coding helps identify urgent items
- [ ] Export functionality works (if implemented)

## Testing Checklist
- [ ] View admin dashboard with various data scenarios
- [ ] Filter missing feedback report by department
- [ ] Filter reminder logs by date range
- [ ] Verify all counts are accurate
- [ ] Test with no data (empty state)
- [ ] Test with large dataset (performance)
- [ ] Verify sort functionality works
- [ ] Test navigation from dashboard to reports
- [ ] Check responsive design on mobile
- [ ] Verify colors and styling are professional

## SQL Queries for Verification
```sql
-- Verify missing feedback report
SELECT e.Name, pr.ScheduledDate, d.Name as Department,
       (SELECT COUNT(*) FROM Feedbacks WHERE ReviewId = pr.Id) as SubmittedCount
FROM PerformanceReviews pr
JOIN Employees e ON pr.EmployeeId = e.Id
LEFT JOIN Departments d ON e.DepartmentId = d.Id
WHERE pr.Status != 'Completed' AND pr.Status != 'Cancelled';

-- Verify reminder logs
SELECT rl.*, pr.ScheduledDate, e.Name
FROM ReminderLogs rl
JOIN PerformanceReviews pr ON rl.ReviewId = pr.Id
JOIN Employees e ON pr.EmployeeId = e.Id
ORDER BY rl.SentAt DESC;
```

## Dependencies
- Iteration 4 must be completed (AdminLayout)
- Iteration 7 must be completed (Reminder logic)

## Notes
- Keep reports simple and focused on key information
- Use Bootstrap table classes for consistent styling
- Consider adding export to Excel/CSV in future
- Charts can be added with simple libraries (Chart.js, ApexCharts)
- Human reviews report designs before implementation
- Ensure queries are optimized for performance
- Consider caching for dashboard summary (future enhancement)
