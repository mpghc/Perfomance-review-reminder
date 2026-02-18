# Stage 5: Pages and Routing

## Objective
Create all Blazor pages with proper routing, navigation, and basic UI structure for the application.

## Description
Implement all user-facing pages for the Performance Review Reminder Bot. This includes employee management, performance review scheduling and tracking, feedback submission, and admin reporting pages. Each page will use dependency injection to access services and will implement proper routing.

## Pages to Create

### 1. Home Page
**Route:** `/` or `/index`
**File:** `Pages/Index.razor`

**Features:**
- Dashboard view with quick statistics
- Count of active employees
- Count of reviews this month
- Count of pending feedback
- Recent activity summary
- Navigation links to main sections

### 2. Employee Management Pages

#### Employee List
**Route:** `/employees`
**File:** `Pages/Employees/Index.razor`

**Features:**
- Display all employees in a table
- Show: Name, Email, Department, Role, IsActive status
- Search/filter by name or email
- Sort by columns
- Links to Edit and Delete actions
- "Create New Employee" button

#### Create Employee
**Route:** `/employees/create`
**File:** `Pages/Employees/Create.razor`

**Features:**
- Form with fields: Name, Email, Department, Role
- Client-side validation
- Submit to EmployeeService.CreateAsync
- Success/error messages
- Cancel button returns to list

#### Edit Employee
**Route:** `/employees/edit/{id:int}`
**File:** `Pages/Employees/Edit.razor`

**Features:**
- Load employee by ID
- Pre-populated form
- Update via EmployeeService.UpdateAsync
- Validation
- Success/error messages
- Cancel button

#### Delete Employee (Optional - can be modal)
**Route:** `/employees/delete/{id:int}` (or modal in Index)
**File:** `Pages/Employees/Delete.razor`

**Features:**
- Show employee details
- Confirmation prompt
- Delete via EmployeeService.DeleteAsync
- Handle cascade delete issues

### 3. Performance Review Pages

#### Review List
**Route:** `/reviews`
**File:** `Pages/Reviews/Index.razor`

**Features:**
- Display all reviews in a table
- Show: Employee Name, Reviewer Name, Scheduled Date, Status, Period
- Filter by status
- Filter by current month
- Sort by scheduled date
- Links to Details, Edit, and Delete
- "Schedule New Review" button

#### Create Review
**Route:** `/reviews/create`
**File:** `Pages/Reviews/Create.razor`

**Features:**
- Form with fields:
  - Employee dropdown (select reviewee)
  - Reviewer dropdown (select reviewer)
  - Scheduled Date picker
  - Period text input
  - Status dropdown (default: Pending)
  - Notes textarea
- Validation (reviewer ≠ reviewee)
- Submit to PerformanceReviewService.CreateAsync
- Success/error messages

#### Review Details
**Route:** `/reviews/{id:int}`
**File:** `Pages/Reviews/Details.razor`

**Features:**
- Show all review information
- Display related employee and reviewer details
- List all feedback submissions for this review
- Show reminder log history
- "Submit Feedback" button
- "Edit Review" button
- Status change buttons (if applicable)

#### Edit Review
**Route:** `/reviews/edit/{id:int}`
**File:** `Pages/Reviews/Edit.razor`

**Features:**
- Load review by ID
- Pre-populated form (similar to Create)
- Update via PerformanceReviewService.UpdateAsync
- Validation
- Success/error messages

### 4. Feedback Page

#### Submit Feedback
**Route:** `/feedback/{reviewId:int}`
**File:** `Pages/Feedback/Submit.razor`

**Features:**
- Show review details (employee, reviewer, period)
- Form with fields:
  - Provider dropdown (current user or select)
  - Content textarea
  - Rating (1-5 stars, optional)
- Validation (prevent duplicate feedback)
- Submit to FeedbackService.SubmitFeedbackAsync
- Success message
- Return to review details

### 5. Admin Pages

#### Missing Feedback Report
**Route:** `/admin/missing-feedback`
**File:** `Pages/Admin/MissingFeedback.razor`

**Features:**
- Display reviews with missing feedback
- Show: Employee, Reviewer, Scheduled Date, Status
- Filter by date range
- Export functionality (optional)
- Send reminder button per review

#### Reminder Log History
**Route:** `/admin/reminders`
**File:** `Pages/Admin/ReminderLogs.razor`

**Features:**
- Display all reminder logs
- Show: Review, Recipient, Sent Date, Type, Message
- Filter by date range
- Filter by review or employee
- Pagination for large datasets

## Tasks
- [ ] Create `Pages/Index.razor` (Home/Dashboard)
- [ ] Create `Pages/Employees/Index.razor` (Employee List)
- [ ] Create `Pages/Employees/Create.razor` (Create Employee)
- [ ] Create `Pages/Employees/Edit.razor` (Edit Employee)
- [ ] Create `Pages/Employees/Delete.razor` or modal (Delete Employee)
- [ ] Create `Pages/Reviews/Index.razor` (Review List)
- [ ] Create `Pages/Reviews/Create.razor` (Create Review)
- [ ] Create `Pages/Reviews/Details.razor` (Review Details)
- [ ] Create `Pages/Reviews/Edit.razor` (Edit Review)
- [ ] Create `Pages/Feedback/Submit.razor` (Submit Feedback)
- [ ] Create `Pages/Admin/MissingFeedback.razor` (Admin Report)
- [ ] Create `Pages/Admin/ReminderLogs.razor` (Reminder Log History)
- [ ] Configure routing for all pages using `@page` directive
- [ ] Inject services into pages using `@inject`
- [ ] Implement navigation between pages
- [ ] Add error boundaries for graceful error handling
- [ ] Add loading indicators for async operations
- [ ] Test all navigation paths

## Acceptance Criteria
- All pages are accessible via their defined routes
- Navigation between pages works correctly
- Forms validate input properly
- Services are injected and used correctly
- Success/error messages display appropriately
- Pages handle loading states (show spinner during data fetch)
- Pages handle errors gracefully
- Pages follow Blazor best practices
- Responsive design (works on mobile and desktop)
- Consistent UI/UX across all pages

## Technical Notes
- Use `@page` directive for routing
- Use `@inject` for dependency injection
- Use `NavigationManager` for programmatic navigation
- Use `EditForm` with validation for forms
- Handle async operations with `Task` and `await`
- Use `OnInitializedAsync` for data loading
- Consider using `StateHasChanged()` when needed
- Implement parameter binding for route parameters

## Dependencies
- Stage 1 (Solution Structure) must be complete
- Stage 2 (Folder Organization) must be complete
- Stage 3 (Entities and Relationships) must be complete
- Stage 4 (Services Definition) must be complete

## Estimated Effort
Large - 6-8 hours
