# Stage 2: Folder Organization

## Objective
Define a clean, maintainable folder structure following simplified layered architecture principles.

## Description
Establish the complete folder hierarchy for the application following a simplified layered architecture pattern. This structure will separate concerns between domain models, data access, business logic, UI pages, and shared components.

## Proposed Structure
```
PerformanceReviewReminder/
├── Models/                    # Domain entities
│   ├── Employee.cs
│   ├── PerformanceReview.cs
│   ├── Feedback.cs
│   └── ReminderLog.cs
├── Data/                      # Database context
│   └── ApplicationDbContext.cs
├── Services/                  # Business logic
│   ├── Interfaces/
│   │   ├── IEmployeeService.cs
│   │   ├── IPerformanceReviewService.cs
│   │   ├── IFeedbackService.cs
│   │   └── IReminderService.cs
│   ├── EmployeeService.cs
│   ├── PerformanceReviewService.cs
│   ├── FeedbackService.cs
│   ├── ReminderService.cs
│   └── ReminderBackgroundService.cs
├── Pages/                     # Blazor pages
│   ├── Employees/
│   ├── Reviews/
│   ├── Feedback/
│   └── Admin/
├── Components/                # Reusable components
│   └── Shared/
├── Shared/                    # Layouts
│   ├── MainLayout.razor
│   ├── AdminLayout.razor
│   └── NavMenu.razor
└── wwwroot/                   # Static files (already exists)
```

## Tasks
- [ ] Create `Models/` folder for domain entities
- [ ] Create `Data/` folder for database context
- [ ] Create `Services/` folder with `Interfaces/` subfolder
- [ ] Create `Pages/Employees/` folder
- [ ] Create `Pages/Reviews/` folder
- [ ] Create `Pages/Feedback/` folder
- [ ] Create `Pages/Admin/` folder
- [ ] Create `Components/Shared/` folder
- [ ] Verify `Shared/` folder exists for layouts
- [ ] Verify `wwwroot/` folder exists for static files
- [ ] Document folder structure in README or architecture doc

## Acceptance Criteria
- All folders are created in the main project
- Folder names follow C# conventions (PascalCase)
- Structure follows simplified layered architecture (UI → Services → Data)
- No unnecessary nesting or complexity
- Folders are organized logically by feature/concern
- README or documentation explains the purpose of each folder

## Technical Notes
- Avoid creating empty placeholder files at this stage
- Follow YAGNI principle - only create folders we know we need
- Keep structure flat where possible
- Subfolders in Pages organize by feature area

## Architecture Principles
- **No CQRS**: Simple CRUD operations in services
- **No MediatR**: Direct service calls from pages
- **No Generic Repository**: DbContext used directly in services
- **No Unit of Work**: Transactions managed explicitly where needed
- **Simplified Layers**: Blazor UI → Services → Data (DbContext)

## Dependencies
- Stage 1 (Solution Structure) must be complete

## Estimated Effort
Small - 15-30 minutes
