# Iteration 5: Employee & Review Management Pages

**Labels:** `iteration`, `ui`, `blazor`, `crud`
**Priority:** High
**Estimated Time:** 2-3 sessions
**Depends on:** Iteration 3, Iteration 4

## Goal
Create pages for managing employees and performance reviews with full CRUD operations.

## Tasks

### Employee Management
- [ ] Create Employees.razor page (@layout MainLayout)
  - Display employees in Bootstrap table
  - Add "New Employee" button
  - Edit button for each employee
  - Delete button with confirmation
  - Search/filter textbox
  - Pagination (optional)
- [ ] Create EmployeeForm.razor component
  - Input fields: Name, Email, Department dropdown
  - Validation messages
  - Save and Cancel buttons
  - Reusable for Create and Edit
- [ ] Create EmployeeDetails.razor page
  - Display employee information
  - List employee's performance reviews
  - Navigation to review details
  - Edit employee button

### Review Management
- [ ] Create Reviews.razor page (@layout MainLayout)
  - Display reviews in Bootstrap table
  - Filter by: Status, Employee, Date range
  - Add "New Review" button
  - View details button
  - Update status dropdown
  - Show review status with badges (color-coded)
- [ ] Create ReviewForm.razor component
  - Select Employee dropdown
  - Select Reviewer dropdown
  - Scheduled Date picker
  - Status dropdown (Scheduled, InProgress, Completed, Cancelled)
  - Validation messages
  - Save and Cancel buttons
- [ ] Create ReviewDetails.razor page
  - Display review information (Employee, Reviewer, Date, Status)
  - List all feedbacks for the review
  - Show missing feedbacks
  - Link to submit feedback
  - Update status button

### Reusable Components
- [ ] Create ConfirmDialog.razor component
  - Modal dialog for confirmations
  - Customizable title and message
  - Yes/No buttons
  - Event callbacks for user response
- [ ] Create LoadingSpinner.razor component
  - Show during async operations
  - Bootstrap spinner
- [ ] Create StatusBadge.razor component
  - Color-coded badges for review status
  - Parameters: Status (string), CssClass (string)

## Form Validation Requirements
- [ ] Employee Name: Required, MaxLength(100)
- [ ] Employee Email: Required, Valid email format, MaxLength(100)
- [ ] Review Employee: Required
- [ ] Review Reviewer: Required
- [ ] Review Scheduled Date: Required, Cannot be in the past
- [ ] Display validation errors prominently

## UI/UX Requirements
- [ ] Loading indicators during async operations
- [ ] Success/error toast messages after operations
- [ ] Confirmation before delete operations
- [ ] Disabled buttons during processing
- [ ] Form reset after successful submission
- [ ] Proper error handling with user-friendly messages
- [ ] Responsive design (mobile-friendly tables)

## Example Page Structure

### Employees.razor
```razor
@page "/employees"
@layout MainLayout
@inject IEmployeeService EmployeeService

<h3>Employees</h3>

<div class="mb-3">
    <input type="text" class="form-control" @bind="searchTerm" placeholder="Search employees..." />
    <button class="btn btn-primary mt-2" @onclick="ShowCreateForm">New Employee</button>
</div>

@if (isLoading)
{
    <LoadingSpinner />
}
else
{
    <table class="table table-striped">
        <thead>
            <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Department</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var employee in filteredEmployees)
            {
                <tr>
                    <td>@employee.Name</td>
                    <td>@employee.Email</td>
                    <td>@employee.Department?.Name</td>
                    <td>
                        <button class="btn btn-sm btn-info" @onclick="() => ShowEditForm(employee)">Edit</button>
                        <button class="btn btn-sm btn-danger" @onclick="() => ConfirmDelete(employee)">Delete</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
}

@if (showForm)
{
    <EmployeeForm Employee="selectedEmployee" 
                  OnSave="HandleSave" 
                  OnCancel="HandleCancel" />
}
```

## Service Injection Pattern
```csharp
@inject IEmployeeService EmployeeService
@inject IDepartmentService DepartmentService
@inject IReviewService ReviewService
@inject NavigationManager NavigationManager
```

## Acceptance Criteria
- [ ] All CRUD operations work for employees
- [ ] All CRUD operations work for reviews
- [ ] Forms validate input properly
- [ ] Confirmation dialogs appear before delete
- [ ] Loading spinners show during async operations
- [ ] Success/error messages displayed to user
- [ ] Tables are sortable (optional) and responsive
- [ ] Search/filter functionality works
- [ ] Navigation between pages works smoothly
- [ ] Services properly injected and used
- [ ] No null reference exceptions
- [ ] UI is clean and professional

## Testing Checklist
- [ ] Create new employee
- [ ] Edit existing employee
- [ ] Delete employee (with confirmation)
- [ ] Search employees by name
- [ ] Create new review
- [ ] View review details
- [ ] Update review status
- [ ] Filter reviews by status
- [ ] Navigate from employee to their reviews
- [ ] Handle validation errors
- [ ] Test on mobile viewport

## Dependencies
- Iteration 3 must be completed (Services)
- Iteration 4 must be completed (Layouts)

## Notes
- Use EditForm with DataAnnotationsValidator for forms
- Implement proper state management in components
- Use EventCallback for child-parent communication
- Keep components focused and reusable
- Human reviews UI mockups before implementation
- Test all CRUD operations thoroughly
