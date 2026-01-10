# HomeWidgets App - Development Plan

## Overview
Blazor Server frontend with .NET Core backend, PostgreSQL database (Docker), GitHub Pipelines, and Unit/Integration Tests.

---

## Tech Stack
- **Frontend**: Blazor Server (Interactive SSR) with MudBlazor
- **Backend**: ASP.NET Core Web API (.NET 10)
- **Database**: PostgreSQL (Dockerized)
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **CI/CD**: GitHub Actions Pipelines
- **Testing**: xUnit (Unit), Integration Tests

---

## Git Branching Strategy (GitFlow)

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready code only |
| `develop` | Integration branch |
| `feature/*` | Individual features (e.g., `feature/phase1-database-setup`) |
| `bugfix/*` | Bug fixes |
| `release/*` | Release preparation |
| `hotfix/*` | Emergency production fixes |

**Workflow**: `feature/*` → PR → `develop` → `release/*` → `main`

See [CONTRIBUTING.md](./CONTRIBUTING.md) for full details.

---

## Testing Strategy (TDD-Light)

Write tests **alongside** code, not strictly before.

| Layer | Test Type | When | Coverage Goal |
|-------|-----------|------|---------------|
| Core/Domain | Unit Tests | With code | 90%+ |
| Application | Unit Tests | With code | 80%+ |
| Infrastructure | Integration Tests | After impl | 60%+ |
| API | Integration Tests | After impl | 70%+ |

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
**Branch**: `feature/phase1-foundation-auth`

### 1.1 Database Setup (PostgreSQL + Docker)
- [ ] Update `compose.yaml` with PostgreSQL service
- [ ] Configure connection strings (Development/Production)
- [ ] Create `HomeWidgets.Infrastructure` project with EF Core

### 1.2 Domain Layer (`HomeWidgets.Core`)
- [ ] Create `User` entity
- [ ] Create `Widget` entity
- [ ] Create `UserWidget` entity (join table for user's widgets + order)
- [ ] Define repository interfaces (IUserRepository, IWidgetRepository)
- [ ] **TESTS**: Unit tests for entity validation logic

### 1.3 JWT Authentication
- [ ] Install JWT packages (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- [ ] Create `AuthService` for token generation/validation
- [ ] Implement login/register endpoints in API
- [ ] Create protected routes in Blazor App
- [ ] Store JWT in HttpOnly cookie or secure storage
- [ ] **TESTS**: Unit tests for AuthService, Integration tests for auth endpoints

---

## Phase 2: Dashboard & Widget System
**Branch**: `feature/phase2-dashboard-widgets`

### 2.1 Widget Library
- [ ] Create seed data for initial widgets
- [ ] Widget types: Clock, Weather, Notes, Calendar, etc.
- [ ] Each widget has: Id, Name, Description, ComponentType, DefaultConfig
- [ ] **TESTS**: Unit tests for widget entity, seed data validation

### 2.2 Dashboard Layout
- [ ] Create `Dashboard.razor` page
- [ ] Implement responsive grid layout using MudBlazor
- [ ] Display user's saved widgets in correct order
- [ ] **TESTS**: Integration tests for dashboard API endpoints

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
- [ ] **TESTS**: Unit tests for widget ordering logic, Integration tests for CRUD

---

## Phase 3: Custom Widget Creator (AddWidget UI)
**Branch**: `feature/phase3-widget-creator`

### 3.1 Guided Widget Creation
- [ ] Step-by-step wizard UI
- [ ] User describes widget purpose
- [ ] System suggests parameters based on widget type
- [ ] Validation against application constraints
- [ ] **TESTS**: Unit tests for validation logic

### 3.2 Widget Templates
- [ ] Pre-defined templates for common widgets
- [ ] Customizable colors, sizes, data sources
- [ ] Preview before adding
- [ ] **TESTS**: Unit tests for template generation

---

## Phase 4: Test Coverage & Quality
**Branch**: `feature/phase4-test-coverage`

### 4.1 Unit Test Completion
- [ ] Ensure 90%+ coverage on Core/Domain
- [ ] Ensure 80%+ coverage on Application layer
- [ ] Add missing edge case tests

### 4.2 Integration Test Completion
- [ ] Test all API endpoints with TestServer
- [ ] Test database operations with Testcontainers (PostgreSQL)
- [ ] Test full authentication flow

### 4.3 Code Quality
- [ ] Add code analyzers (StyleCop, SonarAnalyzer)
- [ ] Fix all warnings
- [ ] Add XML documentation to public APIs

---

## Phase 5: CI/CD Pipeline
**Branch**: `feature/phase5-cicd`

### 5.1 GitHub Actions
- [ ] Create `.github/workflows/ci.yml`
- [ ] Build & test on push/PR to develop
- [ ] Run tests with coverage reporting
- [ ] Docker image build
- [ ] Deploy to hosting (optional: Azure, Railway, etc.)

### 5.2 Branch Protection
- [ ] Protect `main` and `develop` branches
- [ ] Require PR reviews
- [ ] Require passing tests before merge

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
- [x] Git repository initialized
- [x] GitHub repository created (private)
- [x] GitFlow branching strategy implemented (`main` + `develop`)
- [x] CONTRIBUTING.md with branching & testing guidelines
- [ ] **NEXT**: Create `feature/phase1-foundation-auth` branch and start Phase 1

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

