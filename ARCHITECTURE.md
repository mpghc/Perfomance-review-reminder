# Performance Review Reminder Bot - Architecture Proposal

## 1. Solution Structure

```
PerformanceReviewBot/
├── PerformanceReviewBot.Web/          # ASP.NET Core Web Application (Razor Pages)
│   ├── Pages/                          # Razor Pages
│   ├── wwwroot/                        # Static files (CSS, JS)
│   ├── Models/                         # View Models
│   └── Program.cs                      # Application entry point
├── PerformanceReviewBot.Core/         # Core Business Logic
│   ├── Entities/                       # Domain entities
│   ├── Interfaces/                     # Service interfaces
│   └── Services/                       # Business logic services
├── PerformanceReviewBot.Data/         # Data Access Layer
│   ├── ApplicationDbContext.cs         # EF Core DbContext
│   ├── Repositories/                   # Repository implementations
│   └── Migrations/                     # EF Core migrations
└── PerformanceReviewBot.Tests/        # Unit Tests
    ├── ServiceTests/                   # Service layer tests
    └── RepositoryTests/                # Repository tests
```

**Technology Stack:**
- ASP.NET Core 8.0 (Razor Pages)
- Entity Framework Core 8.0
- SQLite database
- xUnit for testing
- Bootstrap 5 for UI

**Layering:**
- **Web Layer**: Razor Pages for UI, handles HTTP requests/responses
- **Core Layer**: Business logic, service interfaces, domain entities
- **Data Layer**: EF Core, repositories, database context
- **Tests Layer**: Unit tests for services and repositories

---

## 2. Entities and Relationships

### Employee Entity
```
Employee
├── Id (int, PK)
├── FirstName (string)
├── LastName (string)
├── Email (string, unique)
├── Department (string)
├── IsActive (bool)
├── CreatedAt (DateTime)
└── Reviews (ICollection<PerformanceReview>)
```

### PerformanceReview Entity
```
PerformanceReview
├── Id (int, PK)
├── EmployeeId (int, FK)
├── ReviewDate (DateTime) - Scheduled review date
├── Status (enum: Pending, Completed, Overdue)
├── FeedbackSubmitted (bool)
├── FeedbackText (string, nullable)
├── ReminderSent (bool)
├── ReminderSentDate (DateTime, nullable)
├── CompletedDate (DateTime, nullable)
├── CreatedAt (DateTime)
└── Employee (Employee, navigation)
```

### ReminderLog Entity
```
ReminderLog
├── Id (int, PK)
├── PerformanceReviewId (int, FK)
├── SentDate (DateTime)
├── RecipientEmail (string)
├── MessageType (enum: EmployeeReminder, ManagerReport)
└── PerformanceReview (PerformanceReview, navigation)
```

**Relationships:**
- One Employee has many PerformanceReviews (1:N)
- One PerformanceReview has many ReminderLogs (1:N)

**Enums:**
- ReviewStatus: Pending, Completed, Overdue
- MessageType: EmployeeReminder, ManagerReport

---

## 3. Pages and Routing Structure

### Main Layout Pages (Employee-facing)
| Route | Page | Purpose |
|-------|------|---------|
| `/` | Index | Dashboard showing upcoming reviews |
| `/Employees` | Employees/Index | List all employees |
| `/Employees/Create` | Employees/Create | Add new employee |
| `/Employees/Edit/{id}` | Employees/Edit | Edit employee details |
| `/Employees/Details/{id}` | Employees/Details | View employee details |
| `/Reviews` | Reviews/Index | List all reviews |
| `/Reviews/Create` | Reviews/Create | Schedule new review |
| `/Reviews/Edit/{id}` | Reviews/Edit | Edit review details |
| `/Reviews/Details/{id}` | Reviews/Details | View review details |
| `/Reviews/SubmitFeedback/{id}` | Reviews/SubmitFeedback | Submit feedback for review |

### Admin Layout Pages
| Route | Page | Purpose |
|-------|------|---------|
| `/Admin` | Admin/Index | Admin dashboard |
| `/Admin/Reports` | Admin/Reports | Missing feedback report |
| `/Admin/Reminders` | Admin/Reminders | View reminder history |
| `/Admin/SendReminders` | Admin/SendReminders | Manually trigger reminders |

---

## 4. Layouts

### Main Layout (`_Layout.cshtml`)
**Features:**
- Simple navigation menu (Home, Employees, Reviews)
- Bootstrap-based responsive design
- Clean, professional look
- Breadcrumb navigation

**Structure:**
```
Header: Company logo + Navigation menu
Body: @RenderBody()
Footer: Copyright info
```

### Admin Layout (`_AdminLayout.cshtml`)
**Features:**
- Admin-specific navigation (Dashboard, Reports, Reminders)
- Highlighted admin section (different color scheme)
- Statistics widgets
- Admin badge/indicator

**Structure:**
```
Header: Admin badge + Admin navigation
Sidebar: Quick stats and filters
Body: @RenderBody()
Footer: Admin tools
```

---

## 5. API Endpoints

While the application uses Razor Pages for the UI, we'll implement minimal API endpoints for AJAX operations:

### Employee Endpoints
- `GET /api/employees` - Get all employees
- `GET /api/employees/{id}` - Get employee by ID
- `POST /api/employees` - Create employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Delete employee

### Review Endpoints
- `GET /api/reviews` - Get all reviews
- `GET /api/reviews/month/{year}/{month}` - Get reviews for specific month
- `POST /api/reviews` - Create review
- `PUT /api/reviews/{id}` - Update review
- `PUT /api/reviews/{id}/feedback` - Submit feedback

### Reminder Endpoints
- `POST /api/reminders/send` - Manually trigger reminders
- `GET /api/reminders/logs` - Get reminder history

---

## 6. Reminder Logic (Monthly Check)

### Implementation Strategy

**Background Service:**
- Create `ReminderBackgroundService` inheriting from `BackgroundService`
- Runs daily at 9:00 AM
- Checks for reviews scheduled in the current month
- Sends reminders to employees who haven't submitted feedback

**Reminder Rules:**
1. Check all reviews with `ReviewDate` in current month
2. Send reminder if:
   - `FeedbackSubmitted == false`
   - `ReminderSent == false` OR last reminder sent > 7 days ago
3. Mark review as `Overdue` if `ReviewDate` has passed and no feedback
4. Send manager report on last day of month with all missing feedback

**Email Simulation:**
- `IEmailService` interface with two implementations:
  - `ConsoleEmailService` (development) - logs to console
  - `SmtpEmailService` (production) - actual email sending (not implemented)
- Use configuration to switch between implementations

**Process Flow:**
```
Daily Task → Check Current Month Reviews → Filter Pending Reviews
→ Send Employee Reminders → Log Reminders → Update Review Status
→ Check End of Month → Generate Manager Report
```

---

## 7. Testing Strategy

### Unit Tests Structure

**Service Tests:**
- `EmployeeServiceTests` - CRUD operations, validation
- `ReviewServiceTests` - Review lifecycle, status transitions
- `ReminderServiceTests` - Reminder logic, scheduling rules
- Mock repositories using interfaces

**Repository Tests:**
- `EmployeeRepositoryTests` - Data access operations
- `ReviewRepositoryTests` - Query filtering, date ranges
- Use in-memory SQLite database for testing

**Test Coverage Goals:**
- Core business logic: 90%+
- Repository layer: 80%+
- Background services: 80%+

**Test Categories:**
1. **Unit Tests**: Individual methods, mocked dependencies
2. **Integration Tests**: Database operations with in-memory DB
3. **End-to-End Tests**: Not required (manual testing sufficient)

**Testing Tools:**
- xUnit as test framework
- Moq for mocking
- FluentAssertions for readable assertions
- In-memory SQLite for repository tests

**Key Test Scenarios:**
- Creating/updating/deleting entities
- Review status transitions (Pending → Completed/Overdue)
- Reminder sending logic (monthly checks, 7-day intervals)
- Manager report generation
- Edge cases: empty data, invalid dates, null handling

---

## Summary

This architecture provides:
✅ Clean separation of concerns (Web, Core, Data layers)
✅ Simple but complete CRUD functionality
✅ Two distinct layouts (Main and Admin)
✅ Multiple pages with routing
✅ SQLite database with EF Core
✅ Background service for reminders
✅ Comprehensive unit tests
✅ Production-ready but minimal code
✅ No overengineering or unnecessary patterns

**Next Steps:**
Awaiting your approval to proceed with implementation.
