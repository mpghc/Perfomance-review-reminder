# Performance Review Reminder Bot

An ASP.NET Core 8 Razor Pages application for managing employees and tracking their performance reviews.

## Features

- **Employee Management** — Full CRUD operations for employees
- **Review Management** — Schedule, track, and manage performance reviews
- **Admin Dashboard** — At-a-glance view of overdue and upcoming reviews
- **Two Layouts** — Main site layout and dedicated Admin panel layout
- **InMemory Database** — EF Core InMemory provider with seed data

## Project Structure

```
PerformanceReviewBot/
├── Data/                  # DbContext and seed data
├── Models/                # Employee and PerformanceReview entities
├── Services/              # Service layer (EmployeeService, ReminderService)
├── Pages/
│   ├── Employees/         # Employee CRUD pages
│   ├── Reviews/           # Review CRUD pages
│   ├── Admin/             # Admin Dashboard
│   └── Shared/            # Layouts (_Layout, _AdminLayout)
├── wwwroot/               # Static files (CSS, JS, Bootstrap)
└── Program.cs             # App configuration and DI
```

## Getting Started

```bash
cd PerformanceReviewBot
dotnet run
```

The app will start on the URL configured in `Properties/launchSettings.json`.

## Tech Stack

- ASP.NET Core 8 Razor Pages
- Entity Framework Core (InMemory provider)
- Bootstrap 5
- jQuery Validation
