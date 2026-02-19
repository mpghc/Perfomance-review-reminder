# Copilot Project Rules

This repository is part of an AI-assisted development course.

Architecture:
- Simplified Clean Architecture
- No extra abstraction layers
- EF Core DbContext used directly in services

Do NOT introduce:
- CQRS
- MediatR
- Generic repository
- Unit of Work
- Event bus
- Advanced scheduling frameworks

Reminder logic must:
- Be simple
- Be deterministic
- Run inside transaction
- Log all operations