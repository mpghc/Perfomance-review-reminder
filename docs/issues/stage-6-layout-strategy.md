# Stage 6: Layout Strategy

## Objective
Create distinct layouts with reusable components and Bootstrap styling for consistent, professional UI.

## Description
Implement two different layouts (MainLayout and AdminLayout) to provide different navigation and visual themes for standard users vs. administrators. Create reusable UI components that can be used across multiple pages. Apply Bootstrap 5 for responsive, clean, and modern design.

## Layouts to Create

### 1. MainLayout.razor
**Purpose:** Standard user interface for regular employees

**Features:**
- Bootstrap navbar with brand logo/name
- Navigation menu with links:
  - Home / Dashboard
  - Employees
  - Performance Reviews
  - Submit Feedback
- User info display (optional)
- Responsive hamburger menu for mobile
- Main content area with container
- Footer with copyright/version info
- Bootstrap theme: Default or light theme

**Layout Structure:**
```razor
<div class="page">
    <header>
        <NavMenu />
    </header>
    <main class="container mt-4">
        @Body
    </main>
    <footer>
        <!-- Footer content -->
    </footer>
</div>
```

### 2. AdminLayout.razor
**Purpose:** Administrator interface for managers and admin users

**Features:**
- Distinct visual theme (different navbar color, e.g., dark theme)
- Navigation menu with links:
  - Admin Dashboard
  - Missing Feedback Report
  - Reminder Logs
  - All Employees (link to main section)
  - All Reviews (link to main section)
- Admin badge or indicator
- Same responsive structure as MainLayout
- Main content area with container
- Footer with admin info

**Layout Differentiation:**
- Different Bootstrap theme/color scheme
- Admin-specific navigation items
- Visual indicators (admin badge, different navbar color)

## Reusable Components to Create

### 1. NavMenu.razor
**Location:** `Shared/NavMenu.razor`

**Features:**
- Renders navigation items
- Highlights active page
- Responsive collapse for mobile
- Bootstrap navbar styling
- Props for customizing appearance

### 2. AlertComponent.razor
**Location:** `Components/Shared/AlertComponent.razor`

**Features:**
- Display success, error, warning, info messages
- Bootstrap alert styling
- Dismissible option
- Auto-hide after timeout (optional)
- Parameters:
  - `AlertType` (success, danger, warning, info)
  - `Message` (string)
  - `IsDismissible` (bool)

### 3. ConfirmDialog.razor
**Location:** `Components/Shared/ConfirmDialog.razor`

**Features:**
- Modal confirmation dialog
- Customizable title and message
- Confirm and Cancel buttons
- Event callbacks for user actions
- Bootstrap modal styling
- Parameters:
  - `Title` (string)
  - `Message` (string)
  - `ConfirmButtonText` (string, default: "Confirm")
  - `OnConfirm` (EventCallback)
  - `OnCancel` (EventCallback)

### 4. LoadingSpinner.razor
**Location:** `Components/Shared/LoadingSpinner.razor`

**Features:**
- Bootstrap spinner animation
- Overlay option for full-page loading
- Customizable size and color
- Parameters:
  - `IsLoading` (bool)
  - `IsOverlay` (bool)
  - `Message` (string, optional)

### 5. DataTable.razor (Optional)
**Location:** `Components/Shared/DataTable.razor`

**Features:**
- Generic table component
- Bootstrap table styling
- Sortable columns
- Pagination support
- Search/filter
- Customizable columns via RenderFragment

## Bootstrap Integration

### Styling Guidelines:
- Use Bootstrap 5 classes consistently
- Color scheme:
  - Primary: Blue (#0d6efd) for main actions
  - Success: Green for confirmations
  - Danger: Red for deletions/errors
  - Warning: Yellow for warnings
  - Info: Cyan for information
- Typography: Use Bootstrap's typography system
- Spacing: Use Bootstrap spacing utilities (mt-3, p-4, etc.)
- Forms: Use Bootstrap form controls and validation
- Tables: Use Bootstrap table classes (table, table-striped, table-hover)
- Buttons: Use Bootstrap button classes (btn, btn-primary, etc.)
- Cards: Use Bootstrap cards for content sections

### Responsive Design:
- Mobile-first approach
- Use Bootstrap grid system (container, row, col-*)
- Breakpoints: xs, sm, md, lg, xl
- Responsive tables (table-responsive)
- Hamburger menu for mobile navigation

## Tasks
- [ ] Create `Shared/MainLayout.razor` with standard user layout
- [ ] Create `Shared/AdminLayout.razor` with admin-specific layout
- [ ] Create `Shared/NavMenu.razor` component for navigation
- [ ] Create `Components/Shared/AlertComponent.razor` for messages
- [ ] Create `Components/Shared/ConfirmDialog.razor` for confirmations
- [ ] Create `Components/Shared/LoadingSpinner.razor` for loading states
- [ ] Configure default layout in `App.razor` or `_Imports.razor`
- [ ] Apply Bootstrap 5 classes consistently across layouts
- [ ] Implement responsive navigation (hamburger menu)
- [ ] Style footer with copyright and version info
- [ ] Test layouts on different screen sizes
- [ ] Create custom CSS overrides in `wwwroot/css/site.css` if needed
- [ ] Ensure accessibility (ARIA labels, keyboard navigation)
- [ ] Add favicon and app icons in `wwwroot/`

## Acceptance Criteria
- Two distinct layouts exist and are visually different
- MainLayout used for standard user pages
- AdminLayout used for admin pages
- Navigation works correctly in both layouts
- All reusable components are functional
- Bootstrap styling is applied consistently
- Responsive design works on mobile, tablet, desktop
- Pages can easily specify which layout to use
- Active page is highlighted in navigation
- Loading states are handled with spinner component
- Alert messages display correctly
- Confirmation dialogs work as expected
- Code follows Blazor component best practices
- Accessibility standards are met

## Technical Notes
- Use `@layout` directive in pages to specify layout
- Use `NavLink` component for navigation with active highlighting
- Use Bootstrap JavaScript for interactive components (modals, dropdowns)
- Consider using Blazor's built-in validation for forms
- Use CSS isolation (`.razor.css` files) where appropriate
- Test on multiple browsers (Chrome, Firefox, Edge)

## Dependencies
- Stage 1 (Solution Structure) must be complete
- Stage 2 (Folder Organization) must be complete
- Stage 5 (Pages and Routing) should be in progress or complete

## Estimated Effort
Medium - 3-4 hours
