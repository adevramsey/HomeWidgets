# HomeWidgets App - Development Plan

## Overview
Blazor Server frontend with .NET Core backend, PostgreSQL database (Docker), GitHub Pipelines, and Unit/Integration Tests.

---

## Tech Stack
- **Frontend**: Blazor Server (Interactive SSR) with MudBlazor
- **Backend**: ASP.NET Core Web API (.NET 8/9+)
- **Database**: PostgreSQL (Dockerized)
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **CI/CD**: GitHub Actions Pipelines
- **Testing**: xUnit (Unit), Integration Tests

---

## Architecture (Clean Architecture)

```
HomeWidgets/
├── src/
│   ├── HomeWidgets.App/          # Blazor Server UI
│   ├── HomeWidgets.Api/          # ASP.NET Core Web API
│   ├── HomeWidgets.Core/         # Domain entities, interfaces (no dependencies)
│   ├── HomeWidgets.Application/  # Use cases, DTOs, services
│   └── HomeWidgets.Infrastructure/ # EF Core, external services, JWT
├── tests/
│   ├── HomeWidgets.UnitTests/
│   └── HomeWidgets.IntegrationTests/
└── docker-compose.yaml
```

---

## Phase 1: Foundation & Authentication

### 1.1 Database Setup (PostgreSQL + Docker)
- [ ] Update `compose.yaml` with PostgreSQL service
- [ ] Configure connection strings (Development/Production)
- [ ] Create `HomeWidgets.Infrastructure` project with EF Core

### 1.2 Domain Layer (`HomeWidgets.Core`)
- [ ] Create `User` entity
- [ ] Create `Widget` entity
- [ ] Create `UserWidget` entity (join table for user's widgets + order)
- [ ] Define repository interfaces (IUserRepository, IWidgetRepository)

### 1.3 JWT Authentication
- [ ] Install JWT packages (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- [ ] Create `AuthService` for token generation/validation
- [ ] Implement login/register endpoints in API
- [ ] Create protected routes in Blazor App
- [ ] Store JWT in HttpOnly cookie or secure storage

---

## Phase 2: Dashboard & Widget System

### 2.1 Widget Library
- [ ] Create seed data for initial widgets
- [ ] Widget types: Clock, Weather, Notes, Calendar, etc.
- [ ] Each widget has: Id, Name, Description, ComponentType, DefaultConfig

### 2.2 Dashboard Layout
- [ ] Create `Dashboard.razor` page
- [ ] Implement responsive grid layout using MudBlazor
- [ ] Display user's saved widgets in correct order

### 2.3 Drag & Drop
- [ ] Implement drag-and-drop reordering (MudBlazor DragDrop or custom JS interop)
- [ ] Save widget order on drop (auto-save)
- [ ] Debounce save calls for performance

### 2.4 Add/Remove Widgets
- [ ] Create "Add Widget" dialog/drawer
- [ ] Show available widgets from library
- [ ] User can add widgets to dashboard
- [ ] User can remove widgets from dashboard
- [ ] Auto-save on add/remove

---

## Phase 3: Custom Widget Creator (AddWidget UI)

### 3.1 Guided Widget Creation
- [ ] Step-by-step wizard UI
- [ ] User describes widget purpose
- [ ] System suggests parameters based on widget type
- [ ] Validation against application constraints

### 3.2 Widget Templates
- [ ] Pre-defined templates for common widgets
- [ ] Customizable colors, sizes, data sources
- [ ] Preview before adding

---

## Phase 4: Testing

### 4.1 Unit Tests
- [ ] Test domain entities
- [ ] Test application services
- [ ] Test authentication logic
- [ ] Mock repositories

### 4.2 Integration Tests
- [ ] Test API endpoints with TestServer
- [ ] Test database operations with in-memory/container DB
- [ ] Test authentication flow

---

## Phase 5: CI/CD Pipeline

### 5.1 GitHub Actions
- [ ] Create `.github/workflows/ci.yml`
- [ ] Build & test on push/PR
- [ ] Docker image build
- [ ] Deploy to hosting (optional: Azure, Railway, etc.)

---

## Development Approach

### Principles We Follow
1. **Clean Architecture**: Separation of concerns, dependency inversion
2. **SOLID Principles**: Single responsibility, open/closed, etc.
3. **Repository Pattern**: Abstract data access
4. **CQRS (optional)**: Separate read/write operations if complexity grows
5. **Small Incremental Changes**: Each PR is reviewable, understandable
6. **Code Reviews**: All code reviewed before merging

### Learning Focus
- Understand each code fragment before moving forward
- Explain the "why" behind design decisions
- Reference best practices from .NET community

---

## Current Status

- [x] Project structure created
- [x] MudBlazor integrated
- [x] Dark theme configured
- [ ] **NEXT**: Phase 1.1 - Database Setup with Docker

---

## Next Steps (Immediate)

1. **Set up PostgreSQL in Docker** - Update `compose.yaml`
2. **Create Core project** - Domain entities
3. **Create Infrastructure project** - EF Core DbContext
4. **Run first migration** - Verify database connection

---

## Questions Before Starting

Before we begin coding:
1. Do you want to use .NET 8 or .NET 9/10 preview?
2. Any specific widget ideas you want in the initial library?
3. Preferred hosting platform for eventual deployment?

