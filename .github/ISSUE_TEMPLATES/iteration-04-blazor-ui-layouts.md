# Iteration 4: Basic Blazor UI & Layouts

**Labels:** `iteration`, `ui`, `blazor`, `layouts`
**Priority:** Medium
**Estimated Time:** 1-2 sessions
**Depends on:** Iteration 1

## Goal
Create the foundational UI structure with routing and layouts.

## Tasks

### Layouts
- [ ] Create MainLayout.razor (standard user view)
  - Navigation sidebar or top nav
  - Header with application title "Performance Review Reminder"
  - Main content area with @Body
  - Footer with copyright/version info
  - Bootstrap styling
- [ ] Create AdminLayout.razor (manager view)
  - Admin navigation menu
  - Header with "Admin Dashboard" title
  - Different color scheme/styling from MainLayout
  - Main content area with @Body
  - Admin-specific footer

### Navigation Components
- [ ] Create NavMenu.razor for MainLayout
  - Home link
  - Employees link
  - Reviews link
  - My Feedbacks link
  - Responsive (collapsible on mobile)
- [ ] Create AdminNavMenu.razor for AdminLayout
  - Dashboard link
  - Missing Feedback Report link
  - Reminder Log Report link
  - Back to User View link

### Configuration
- [ ] Update _Imports.razor with common using statements
  - Add services namespaces
  - Add models namespaces
  - Add component namespaces
- [ ] Update App.razor for proper routing
- [ ] Create Index.razor (home page)
  - Welcome message
  - Brief app description
  - Quick stats (optional)
- [ ] Add custom CSS in wwwroot/css/site.css
  - Layout specific styles
  - Component styles
  - Responsive design rules

## Layout Structure

### MainLayout.razor
```razor
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <h3>Performance Review Reminder</h3>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

### AdminLayout.razor
```razor
@inherits LayoutComponentBase

<div class="page admin-layout">
    <div class="sidebar admin-sidebar">
        <AdminNavMenu />
    </div>

    <main>
        <div class="top-row px-4 admin-header">
            <h3>Admin Dashboard</h3>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

## Routing Examples
```csharp
// Standard pages use MainLayout
@page "/"
@layout MainLayout

// Admin pages use AdminLayout
@page "/admin/dashboard"
@layout AdminLayout
```

## Bootstrap Classes to Use
- `navbar`, `navbar-expand-sm` for navigation
- `container`, `container-fluid` for content
- `row`, `col-*` for grid layout
- `card`, `card-body` for panels
- `btn`, `btn-primary`, `btn-secondary` for buttons
- `table`, `table-striped` for tables
- `alert`, `alert-info` for messages

## Acceptance Criteria
- [ ] MainLayout renders correctly in browser
- [ ] AdminLayout renders with distinct styling
- [ ] Navigation works between pages
- [ ] Layouts are responsive (test on different screen sizes)
- [ ] Bootstrap classes applied consistently
- [ ] No console errors in browser
- [ ] Routing configuration works properly
- [ ] NavMenu highlights active page
- [ ] Mobile menu collapses appropriately
- [ ] CSS follows consistent naming convention

## Testing Commands
```bash
# Build and run
dotnet build
dotnet run

# Open browser to https://localhost:5001 or http://localhost:5000
# Test navigation
# Test responsive design (resize browser)
# Check browser console for errors
```

## Dependencies
- Iteration 1 must be completed

## Notes
- Keep layouts simple initially
- Focus on structure over fancy styling
- Ensure accessibility (semantic HTML, ARIA labels)
- Test on multiple browsers if possible
- Human reviews layout mockups before implementation
- Use Blazor's built-in NavLink component for active page highlighting
