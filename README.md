# Performance Review Reminder Bot

A Blazor Server application for managing performance reviews with automated reminders.

## 🚀 Quick Start

### Create GitHub Issues for Project Stages

This project is organized into 8 implementation stages. To create GitHub issues for all stages:

```bash
./create-issues.sh
```

**Prerequisites:**
- [GitHub CLI (`gh`)](https://cli.github.com/) installed and authenticated

For more details, see [docs/issues/README.md](docs/issues/README.md)

## 📋 Project Stages

1. **Stage 1: Solution Structure** - Set up .NET solution and projects
2. **Stage 2: Folder Organization** - Define folder structure
3. **Stage 3: Entities and Relationships** - Create domain models
4. **Stage 4: Services Definition** - Implement business logic
5. **Stage 5: Pages and Routing** - Build Blazor pages
6. **Stage 6: Layout Strategy** - Create layouts and components
7. **Stage 7: Reminder Flow** - Implement reminder system
8. **Stage 8: Testing Strategy** - Write comprehensive tests

## 🛠️ Technology Stack

- **.NET 8** (LTS)
- **Blazor Server** - For interactive web UI
- **Entity Framework Core 8** - ORM for data access
- **SQLite** - Lightweight database
- **xUnit** - Unit testing framework
- **Bootstrap 5** - UI styling

## 🏗️ Architecture

Simplified layered architecture:
- **Blazor UI** → **Services** → **Data (DbContext)**
- No CQRS, MediatR, or generic repository patterns
- Direct DbContext usage in services
- Explicit transactions for multi-entity updates
- YAGNI principle applied throughout

## 📚 Documentation

- [Issue Templates](docs/issues/) - Detailed specifications for each stage
- [C# Coding Guidelines](.github/instructions/csharp.instructions.md)
- [Blazor Guidelines](.github/instructions/blazor.instructions.md)

## 🎯 Project Goals

- Build a working application with 90%+ AI-generated code
- Demonstrate AI-assisted development workflow
- Maintain production-quality, readable code
- Follow .NET and Blazor best practices

## 👥 Contributing

1. Review the stage issues
2. Pick a stage to work on
3. Follow the acceptance criteria
4. Submit a pull request

## 📄 License

[Add license information here]
