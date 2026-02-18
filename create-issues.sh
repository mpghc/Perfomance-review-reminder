#!/bin/bash

# GitHub Issues Creation Script
# This script creates GitHub issues for the Performance Review Reminder Bot project
# Run this script after reviewing the GITHUB_ISSUES_PLAN.md document

REPO="mpghc/Perfomance-review-reminder"

echo "Creating GitHub issues for Performance Review Reminder Bot..."
echo "Repository: $REPO"
echo ""

# Check if gh CLI is available
if ! command -v gh &> /dev/null; then
    echo "Error: GitHub CLI (gh) is not installed."
    echo "Please install it from: https://cli.github.com/"
    exit 1
fi

# Check if authenticated
if ! gh auth status &> /dev/null; then
    echo "Error: Not authenticated with GitHub CLI."
    echo "Please run: gh auth login"
    exit 1
fi

echo "Creating labels..."
gh label create "setup" --description "Initial project setup" --color "0366d6" --repo "$REPO" 2>/dev/null || echo "Label 'setup' already exists"
gh label create "backend" --description "Backend/service layer work" --color "d73a4a" --repo "$REPO" 2>/dev/null || echo "Label 'backend' already exists"
gh label create "frontend" --description "Frontend/UI work" --color "1d76db" --repo "$REPO" 2>/dev/null || echo "Label 'frontend' already exists"
gh label create "ui" --description "User interface improvements" --color "c2e0c6" --repo "$REPO" 2>/dev/null || echo "Label 'ui' already exists"
gh label create "database" --description "Database and migrations" --color "fbca04" --repo "$REPO" 2>/dev/null || echo "Label 'database' already exists"
gh label create "testing" --description "Test-related work" --color "bfd4f2" --repo "$REPO" 2>/dev/null || echo "Label 'testing' already exists"
gh label create "deployment" --description "Deployment preparation" --color "5319e7" --repo "$REPO" 2>/dev/null || echo "Label 'deployment' already exists"
gh label create "admin" --description "Admin-specific features" --color "d4c5f9" --repo "$REPO" 2>/dev/null || echo "Label 'admin' already exists"
gh label create "high-priority" --description "High priority work" --color "b60205" --repo "$REPO" 2>/dev/null || echo "Label 'high-priority' already exists"

echo ""
echo "Creating milestones..."
gh api repos/$REPO/milestones -f title="Foundation & Core" -f description="Project structure, domain model, database setup" -f due_on="2026-02-25T00:00:00Z" 2>/dev/null || echo "Milestone 1 may already exist"
gh api repos/$REPO/milestones -f title="Business Logic" -f description="Service layer and reminder logic" -f due_on="2026-03-04T00:00:00Z" 2>/dev/null || echo "Milestone 2 may already exist"
gh api repos/$REPO/milestones -f title="User Interface" -f description="Layouts, components, and all CRUD pages" -f due_on="2026-03-18T00:00:00Z" 2>/dev/null || echo "Milestone 3 may already exist"
gh api repos/$REPO/milestones -f title="Quality & Release" -f description="Testing, polish, and documentation" -f due_on="2026-03-25T00:00:00Z" 2>/dev/null || echo "Milestone 4 may already exist"

echo ""
echo "Creating issues..."

# Issue #1
gh issue create --repo "$REPO" \
  --title "Set up .NET 8 Blazor Server project structure" \
  --body "Create the foundational structure for the Performance Review Reminder Bot application using .NET 8 and Blazor Server.

**Tasks:**
- [ ] Create .NET 8 Blazor Server application
- [ ] Set up folder structure (Data, Services, Pages)
- [ ] Install NuGet packages (EF Core SQLite, Design, Tools)
- [ ] Configure appsettings.json
- [ ] Set up Program.cs with dependency injection
- [ ] Add Bootstrap styling
- [ ] Create .editorconfig

**Estimated Effort:** 1 hour

**Reference:** See ITERATION_STAGES.md - Stage 1" \
  --label "setup,enhancement"

# Issue #2
gh issue create --repo "$REPO" \
  --title "Implement domain entities for the application" \
  --body "Create all domain entities (Employee, PerformanceReview, Feedback, ReminderLog) with proper relationships and validation.

**Tasks:**
- [ ] Create Data/Entities folder
- [ ] Implement Employee entity
- [ ] Implement PerformanceReview entity
- [ ] Implement Feedback entity
- [ ] Implement ReminderLog entity
- [ ] Create ReviewStatus enum
- [ ] Add XML documentation
- [ ] Configure entity relationships

**Estimated Effort:** 2 hours

**Reference:** See ITERATION_STAGES.md - Stage 2" \
  --label "enhancement,backend"

# Issue #3
gh issue create --repo "$REPO" \
  --title "Set up EF Core DbContext and database migrations" \
  --body "Configure Entity Framework Core with SQLite, create DbContext, and generate initial database schema.

**Tasks:**
- [ ] Create ApplicationDbContext
- [ ] Configure DbSet properties
- [ ] Configure model relationships in OnModelCreating
- [ ] Add indexes for performance
- [ ] Create initial EF Core migration
- [ ] Apply migration to create SQLite database
- [ ] Verify database schema

**Estimated Effort:** 2 hours

**Reference:** See ITERATION_STAGES.md - Stage 3" \
  --label "enhancement,database"

# Issue #4
gh issue create --repo "$REPO" \
  --title "Implement CRUD services for employees, reviews, and feedback" \
  --body "Create service classes for business logic with full CRUD operations for employees, performance reviews, and feedback.

**Tasks:**
- [ ] Implement EmployeeService with CRUD methods
- [ ] Implement PerformanceReviewService with CRUD methods
- [ ] Implement FeedbackService with CRUD methods
- [ ] Register services in Program.cs
- [ ] Add error handling
- [ ] Add XML documentation
- [ ] Manual testing of services

**Estimated Effort:** 4 hours

**Reference:** See ITERATION_STAGES.md - Stage 4" \
  --label "enhancement,backend"

# Issue #5
gh issue create --repo "$REPO" \
  --title "Implement reminder service and background service" \
  --body "Create the core reminder logic that checks for reviews, identifies missing feedback, and logs reminders. Implement background service for daily execution.

**Tasks:**
- [ ] Create ReminderService with CheckAndSendRemindersAsync
- [ ] Implement logic to identify missing feedback
- [ ] Implement reminder logging
- [ ] Ensure transactional consistency
- [ ] Create ReminderBackgroundService
- [ ] Configure background service execution
- [ ] Add reminder settings to configuration
- [ ] Test reminder logic

**Estimated Effort:** 3 hours

**Reference:** See ITERATION_STAGES.md - Stage 5" \
  --label "enhancement,backend,high-priority"

# Issue #6
gh issue create --repo "$REPO" \
  --title "Create Blazor layouts and reusable components" \
  --body "Build the UI foundation with MainLayout, AdminLayout, and reusable components for employees and reviews.

**Tasks:**
- [ ] Create MainLayout.razor
- [ ] Create AdminLayout.razor
- [ ] Update NavMenu.razor for both layouts
- [ ] Create EmployeeCard component
- [ ] Create ReviewStatusBadge component
- [ ] Add Bootstrap styling
- [ ] Configure routing
- [ ] Create _Imports.razor

**Estimated Effort:** 3 hours

**Reference:** See ITERATION_STAGES.md - Stage 6" \
  --label "enhancement,frontend,ui"

# Issue #7
gh issue create --repo "$REPO" \
  --title "Implement employee management pages (CRUD)" \
  --body "Create full CRUD interface for managing employees with list, create, edit, and delete pages.

**Tasks:**
- [ ] Create Employees/Index.razor (list)
- [ ] Create Employees/Create.razor (form)
- [ ] Create Employees/Edit.razor (form)
- [ ] Create Employees/Delete.razor (confirmation)
- [ ] Add validation
- [ ] Add routing
- [ ] Add error handling and user feedback
- [ ] Manual UI testing

**Estimated Effort:** 4 hours

**Reference:** See ITERATION_STAGES.md - Stage 7" \
  --label "enhancement,frontend,ui"

# Issue #8
gh issue create --repo "$REPO" \
  --title "Implement performance review management pages" \
  --body "Create UI for managing performance reviews including list, create, edit, and details pages.

**Tasks:**
- [ ] Create Reviews/Index.razor (list with filters)
- [ ] Create Reviews/Create.razor (form)
- [ ] Create Reviews/Edit.razor (form)
- [ ] Create Reviews/Details.razor (with feedback display)
- [ ] Add routing and navigation
- [ ] Add validation
- [ ] Manual UI testing

**Estimated Effort:** 4 hours

**Reference:** See ITERATION_STAGES.md - Stage 8" \
  --label "enhancement,frontend,ui"

# Issue #9
gh issue create --repo "$REPO" \
  --title "Implement feedback submission and viewing pages" \
  --body "Create UI for submitting and viewing feedback on performance reviews.

**Tasks:**
- [ ] Create Feedback/Index.razor (list)
- [ ] Create Feedback/Submit.razor (form)
- [ ] Integrate feedback submission from review details
- [ ] Add validation
- [ ] Add success notifications
- [ ] Manual UI testing

**Estimated Effort:** 2 hours

**Reference:** See ITERATION_STAGES.md - Stage 9" \
  --label "enhancement,frontend,ui"

# Issue #10
gh issue create --repo "$REPO" \
  --title "Implement admin dashboard and missing feedback report" \
  --body "Create admin-specific pages including a dashboard with statistics and a report showing reviews with missing feedback.

**Tasks:**
- [ ] Create Admin/Dashboard.razor (statistics)
- [ ] Create Admin/MissingFeedback.razor (report)
- [ ] Apply AdminLayout
- [ ] Add admin navigation menu
- [ ] Implement statistics calculations
- [ ] Add filtering options
- [ ] Manual UI testing

**Estimated Effort:** 3 hours

**Reference:** See ITERATION_STAGES.md - Stage 10" \
  --label "enhancement,frontend,ui,admin"

# Issue #11
gh issue create --repo "$REPO" \
  --title "Implement comprehensive unit tests for service layer" \
  --body "Create a test project with xUnit and implement comprehensive unit tests for all services using in-memory database.

**Tasks:**
- [ ] Create test project PerformanceReviewBot.Tests
- [ ] Add xUnit and test dependencies
- [ ] Create InMemoryDbContextFactory
- [ ] Implement EmployeeServiceTests
- [ ] Implement PerformanceReviewServiceTests
- [ ] Implement FeedbackServiceTests
- [ ] Implement ReminderServiceTests
- [ ] Verify all tests pass
- [ ] Aim for high code coverage

**Estimated Effort:** 5 hours

**Reference:** See ITERATION_STAGES.md - Stage 11" \
  --label "testing,backend"

# Issue #12
gh issue create --repo "$REPO" \
  --title "Integration testing, bug fixes, and UI/UX polish" \
  --body "Test end-to-end workflows, fix integration bugs, improve error handling, and polish the UI/UX.

**Tasks:**
- [ ] Test end-to-end workflows
- [ ] Fix integration bugs
- [ ] Improve error handling
- [ ] Add loading indicators
- [ ] Improve error messages
- [ ] Add tooltips and help text
- [ ] Update README with setup instructions
- [ ] Add code documentation
- [ ] Clean up unused code
- [ ] Final code review

**Estimated Effort:** 4 hours

**Reference:** See ITERATION_STAGES.md - Stage 12" \
  --label "enhancement,bug,documentation,ui"

# Issue #13
gh issue create --repo "$REPO" \
  --title "Create final documentation and prepare for deployment" \
  --body "Create comprehensive documentation including deployment guide, user manual, and troubleshooting guide.

**Tasks:**
- [ ] Create deployment guide
- [ ] Document configuration options
- [ ] Create user manual
- [ ] Add troubleshooting guide
- [ ] Create demo data script
- [ ] Final quality assurance
- [ ] Package application
- [ ] Handoff preparation

**Estimated Effort:** 2 hours

**Reference:** See ITERATION_STAGES.md - Stage 13" \
  --label "documentation,deployment"

echo ""
echo "✅ All issues created successfully!"
echo ""
echo "Next steps:"
echo "1. Review the created issues in GitHub"
echo "2. Assign issues to team members"
echo "3. Start with Issue #1 (Project Foundation)"
echo "4. Follow the iteration stages in ITERATION_STAGES.md"
