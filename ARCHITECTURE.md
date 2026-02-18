# SkillMiner - Architecture Proposal

## Project Idea

**SkillMiner** - A Skill Management System for HR departments

A web application that allows HR staff to:
- Extract and manage employee skills from uploaded CVs and documents
- Maintain a searchable database of employee skills
- Track skill levels and certifications
- Generate reports on team capabilities

This is realistic because:
- HR departments need such tools
- It has clear CRUD operations (Create/Read/Update/Delete skills and employees)
- It's small enough for a course project but complete enough to demonstrate competency
- It naturally requires different user roles (Admin vs Regular User views)

---

## 1. Architecture Overview

### Technology Stack
- **Backend**: ASP.NET Core 8.0 (Web API + Razor Pages in same project for simplicity)
- **Frontend**: Razor Pages with minimal JavaScript (no complex SPA framework)
- **Database**: SQLite with Entity Framework Core
- **Testing**: xUnit + Moq

### Solution Structure
```
SkillMiner/
├── SkillMiner.Web/              # Main web application
│   ├── Controllers/             # API controllers
│   ├── Pages/                   # Razor Pages
│   │   ├── Shared/              # Shared components and layouts
│   │   │   ├── _Layout.cshtml           # Main layout
│   │   │   ├── _AdminLayout.cshtml      # Admin layout
│   │   │   └── Components/              # Reusable components
│   │   ├── Index.cshtml                 # Home page
│   │   ├── Employees/                   # Employee pages
│   │   └── Admin/                       # Admin pages
│   ├── Models/                  # Domain entities
│   ├── Data/                    # EF Core DbContext and migrations
│   ├── Services/                # Business logic
│   ├── wwwroot/                 # Static files (CSS, JS)
│   └── appsettings.json         # Configuration
├── SkillMiner.Tests/            # Unit tests
│   ├── Controllers/             # API controller tests
│   ├── Services/                # Service layer tests
│   └── Models/                  # Model validation tests
└── SkillMiner.sln               # Solution file
```

### Layering (Simple 3-Layer)
1. **Presentation Layer** (Pages + Controllers) - User interface and API endpoints
2. **Business Layer** (Services) - Business logic and data validation
3. **Data Layer** (Data + Models) - Database access and entities

---

## 2. Main Entities

### Employee
```
- Id (int, PK)
- FirstName (string, required)
- LastName (string, required)
- Email (string, required, unique)
- Department (string)
- Position (string)
- HireDate (DateTime)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
```

### Skill
```
- Id (int, PK)
- Name (string, required, unique)
- Category (string) // e.g., "Programming", "Language", "Soft Skills"
- Description (string)
- CreatedAt (DateTime)
```

### EmployeeSkill (Join table for many-to-many)
```
- Id (int, PK)
- EmployeeId (int, FK)
- SkillId (int, FK)
- ProficiencyLevel (enum) // Beginner, Intermediate, Advanced, Expert
- YearsOfExperience (decimal)
- LastUsedDate (DateTime, nullable)
- Notes (string, nullable)
- AddedAt (DateTime)
```

### Relationships
- One Employee can have many EmployeeSkills
- One Skill can be linked to many EmployeeSkills
- Many-to-many between Employee and Skill through EmployeeSkill

---

## 3. Pages and Routing Structure

### Main Layout Pages (Public/User View)

| Page | Route | Purpose |
|------|-------|---------|
| Home | `/` | Landing page with search and stats |
| Employee List | `/Employees` | Browse all employees |
| Employee Details | `/Employees/Details/{id}` | View employee profile and skills |
| Skill List | `/Skills` | Browse all skills in system |
| Skill Details | `/Skills/Details/{id}` | View skill details and who has it |
| Search Results | `/Search?q={query}` | Search employees by skills |

### Admin Layout Pages (Administrative View)

| Page | Route | Purpose |
|------|-------|---------|
| Admin Dashboard | `/Admin` | Statistics and quick actions |
| Manage Employees | `/Admin/Employees` | CRUD operations for employees |
| Add Employee | `/Admin/Employees/Create` | Create new employee |
| Edit Employee | `/Admin/Employees/Edit/{id}` | Edit employee details |
| Manage Skills | `/Admin/Skills` | CRUD operations for skills |
| Add Skill | `/Admin/Skills/Create` | Create new skill |
| Edit Skill | `/Admin/Skills/Edit/{id}` | Edit skill details |
| Assign Skills | `/Admin/Employees/{id}/Skills` | Manage employee-skill assignments |

---

## 4. Layouts

### Main Layout (`_Layout.cshtml`)
**Purpose**: For public/read-only pages

**Features**:
- Clean, professional header with company logo
- Navigation menu: Home, Employees, Skills, Search
- Simple footer with copyright
- Light color scheme
- Minimal design

### Admin Layout (`_AdminLayout.cshtml`)
**Purpose**: For administrative/management pages

**Features**:
- Admin-specific header with admin badge
- Sidebar navigation with admin sections
- Breadcrumb navigation
- Action buttons (Create, Edit, Delete)
- Slightly different color scheme (e.g., blue accent for admin)
- Quick stats widgets on dashboard

### Shared Components
- **SkillBadge**: Reusable component to display a skill with level
- **EmployeeCard**: Reusable component to display employee summary
- **ConfirmDialog**: Reusable component for delete confirmations
- **SearchBox**: Reusable search input component

---

## 5. API Endpoints

### Base URL: `/api`

#### Employees API
```
GET    /api/employees              - List all employees (with optional filters)
GET    /api/employees/{id}         - Get employee by ID
POST   /api/employees              - Create new employee
PUT    /api/employees/{id}         - Update employee
DELETE /api/employees/{id}         - Delete employee
GET    /api/employees/{id}/skills  - Get employee's skills
```

#### Skills API
```
GET    /api/skills                 - List all skills (with optional filters)
GET    /api/skills/{id}            - Get skill by ID
POST   /api/skills                 - Create new skill
PUT    /api/skills/{id}            - Update skill
DELETE /api/skills/{id}            - Delete skill
GET    /api/skills/{id}/employees  - Get employees with this skill
```

#### EmployeeSkills API
```
POST   /api/employees/{id}/skills  - Add skill to employee
PUT    /api/employees/{empId}/skills/{skillId} - Update skill assignment
DELETE /api/employees/{empId}/skills/{skillId} - Remove skill from employee
```

#### Search API
```
GET    /api/search?q={query}       - Search employees and skills
GET    /api/search/employees?skill={skillName} - Find employees by skill
```

### Response Format
- Success: 200 OK with data
- Created: 201 Created with location header
- Not Found: 404 with error message
- Validation Error: 400 with validation details
- Server Error: 500 with error message

---

## 6. Testing Strategy

### Unit Tests (Target: 90%+ coverage)

#### 1. Service Layer Tests
**EmployeeService Tests**:
- `CreateEmployee_ValidData_ReturnsEmployee`
- `CreateEmployee_DuplicateEmail_ThrowsException`
- `GetEmployee_ExistingId_ReturnsEmployee`
- `GetEmployee_NonExistingId_ReturnsNull`
- `UpdateEmployee_ValidData_UpdatesSuccessfully`
- `DeleteEmployee_ExistingId_DeletesSuccessfully`
- `DeleteEmployee_WithSkills_DeletesCascade`

**SkillService Tests**:
- `CreateSkill_ValidData_ReturnsSkill`
- `CreateSkill_DuplicateName_ThrowsException`
- `GetSkillsByCategory_ValidCategory_ReturnsSkills`
- `UpdateSkill_ValidData_UpdatesSuccessfully`
- `DeleteSkill_InUse_ThrowsException` (business rule)

**EmployeeSkillService Tests**:
- `AssignSkill_ValidData_AssignsSuccessfully`
- `AssignSkill_AlreadyExists_ThrowsException`
- `UpdateSkillLevel_ValidData_UpdatesSuccessfully`
- `RemoveSkill_ExistingAssignment_RemovesSuccessfully`

#### 2. API Controller Tests
**EmployeesController Tests**:
- `GetAll_ReturnsOkWithEmployees`
- `GetById_ExistingId_ReturnsOkWithEmployee`
- `GetById_NonExistingId_ReturnsNotFound`
- `Post_ValidEmployee_ReturnsCreated`
- `Post_InvalidData_ReturnsBadRequest`
- `Put_ValidUpdate_ReturnsOk`
- `Delete_ExistingId_ReturnsNoContent`

**SkillsController Tests**: (Similar pattern)

#### 3. Model Validation Tests
- Test required fields
- Test string length constraints
- Test email format validation
- Test date validations

#### 4. Repository/Data Layer Tests (Optional, keep simple)
- Test basic CRUD with in-memory database
- Test relationships and cascading

### Integration Tests (Minimal)
- Test complete API workflow: Create employee → Add skills → Retrieve
- Test database migrations run successfully

### Testing Tools
- **xUnit**: Test framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Better assertion syntax
- **In-Memory Database**: For data layer tests

### What NOT to Test
- EF Core internals (already tested by Microsoft)
- ASP.NET Core framework features
- Third-party libraries
- UI rendering (keep it simple, no Selenium needed)

---

## Additional Considerations

### Database
- SQLite file: `skillminer.db` in app directory
- Migrations managed by EF Core
- Seed data for demo purposes (a few employees and skills)

### Validation
- Data annotations on models
- FluentValidation for complex rules (if needed)
- Client-side validation with unobtrusive JavaScript

### Error Handling
- Global exception handler middleware
- Consistent error response format
- Logging to console (simple)

### Security (Basic)
- No authentication for simplicity (course project)
- Input validation to prevent injection
- CSRF protection (built-in with Razor Pages)

### Performance
- Keep it simple, no caching needed
- Pagination for list views (10-20 items per page)
- Eager loading for related entities

---

## Why This Architecture?

✅ **Simple**: All in one project, no microservices, minimal dependencies
✅ **Complete**: Has all required features (CRUD, DB, API, layouts, routing)
✅ **Testable**: Clean separation allows easy unit testing
✅ **Realistic**: Represents real-world HR tool
✅ **Runnable**: Single command to run (`dotnet run`)
✅ **AI-Friendly**: Clear structure makes it easy to generate code systematically

---

## Next Steps (Awaiting Approval)

Once you approve this architecture, I will:
1. Create the solution and project structure
2. Set up the database with Entity Framework Core
3. Generate all entity models
4. Create the service layer with business logic
5. Implement API controllers
6. Build Razor Pages with both layouts
7. Add reusable components
8. Write comprehensive unit tests
9. Verify everything runs correctly

**Estimated Generation**: ~80-100 files, all production-ready code

---

**Please review and approve this architecture before I proceed with implementation.**
