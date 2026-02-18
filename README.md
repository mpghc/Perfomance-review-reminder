# Performance Review Reminder Bot

## Overview

A company-internal AI-driven application that automates performance review tracking and reminder notifications. This system helps managers and employees stay on top of scheduled performance reviews by automatically identifying missing feedback and sending timely reminders.

## Project Status

🚧 **In Development** - Currently in planning and architecture phase

## Technology Stack

- **.NET 8 (LTS)** - Primary framework
- **Blazor Server** - Interactive web UI
- **Entity Framework Core 8** - Data access
- **SQLite** - Database
- **xUnit** - Testing framework
- **Bootstrap** - UI styling

## Key Features

- **Employee Management**: Full CRUD operations for managing employees
- **Performance Review Scheduling**: Create and track performance reviews
- **Feedback Collection**: Submit and view feedback for reviews
- **Automated Reminders**: Daily background service checks for missing feedback
- **Admin Dashboard**: Manager view with statistics and reports
- **Missing Feedback Report**: Identify reviews requiring attention

## Architecture

The application follows a simplified three-layer architecture:
- **UI Layer**: Blazor Server pages and components
- **Service Layer**: Business logic and transactions
- **Data Layer**: EF Core DbContext and entities

See [architecture.md](./architecture.md) for detailed architectural decisions and design.

## Development Plan

The project is being developed in iterative stages:

1. **Foundation**: Project setup and structure
2. **Domain & Data**: Entities, DbContext, migrations
3. **Services**: Business logic implementation
4. **UI**: Blazor pages and components
5. **Testing**: Unit tests for services
6. **Polish**: Integration and documentation

See [ITERATION_STAGES.md](./ITERATION_STAGES.md) for detailed stage breakdown.

## Documentation

- [Architecture](./architecture.md) - Detailed system architecture and design decisions
- [Iteration Stages](./ITERATION_STAGES.md) - Phased development plan
- [GitHub Issues Plan](./GITHUB_ISSUES_PLAN.md) - Issue tracking strategy

## Getting Started

> **Note**: The application is not yet built. Setup instructions will be added once the implementation is complete.

## Project Goals

- Deliver a working application with **90%+ AI-generated code**
- Production-quality, maintainable codebase
- Comprehensive unit tests
- Clean, user-friendly interface
- Simple, readable architecture (YAGNI principle)

## License

Internal company project - not for public distribution.

---

**Last Updated**: 2026-02-18
