# Iteration 1: Project Setup & Infrastructure

**Labels:** `iteration`, `setup`, `infrastructure`
**Priority:** High
**Estimated Time:** 1 session

## Goal
Set up the foundational .NET 8 Blazor Server project with proper structure and dependencies.

## Architecture Rules
- Simplified layered architecture: Blazor UI → Services → Data
- No CQRS, MediatR, generic repository, or Unit of Work
- Keep code simple and production-quality

## Tasks
- [ ] Create .NET 8 Blazor Server solution and project using `dotnet new blazorserver`
- [ ] Set up folder structure following layered architecture
- [ ] Install required NuGet packages:
  - Microsoft.EntityFrameworkCore.Sqlite (8.x)
  - Microsoft.EntityFrameworkCore.Tools (8.x)
  - xUnit (2.x)
  - xUnit.runner.visualstudio (2.x)
  - Coverlet.collector (for code coverage)
- [ ] Configure project settings and dependencies
- [ ] Create test project structure using `dotnet new xunit`
- [ ] Document folder structure in README
- [ ] Verify build and run

## Expected Folder Structure
```
PerformanceReviewReminder/
├── Pages/              # Blazor pages (.razor)
├── Components/         # Reusable Blazor components
├── Layouts/           # MainLayout.razor, AdminLayout.razor
├── Services/          # Business logic services
├── Data/              # ApplicationDbContext and migrations
├── Models/            # Domain entities (Employee, Review, Feedback, etc.)
├── wwwroot/           # Static files, CSS, JS
├── Program.cs
└── appsettings.json

PerformanceReviewReminder.Tests/
├── Services/          # Service layer unit tests
└── Helpers/           # Test helpers and fixtures
```

## Acceptance Criteria
- [ ] Solution builds successfully (`dotnet build`)
- [ ] Blazor app runs without errors (`dotnet run`)
- [ ] All dependencies installed and referenced
- [ ] Folder structure created and documented
- [ ] Test project can be run (`dotnet test`)
- [ ] README updated with setup instructions

## Technology Stack
- .NET 8 (LTS)
- Blazor Server
- EF Core 8
- SQLite
- xUnit
- Bootstrap

## Dependencies
None - This is the first iteration

## Notes
- Focus on clean structure
- Follow .NET conventions
- Keep it simple (YAGNI principle)
- Human acts as reviewer for structure approval before code generation
