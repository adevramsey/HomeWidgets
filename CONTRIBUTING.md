# Contributing to HomeWidgets

## Git Branching Strategy (GitFlow)

We follow **GitFlow** - an enterprise-standard branching model.

### Branch Types

| Branch | Purpose | Created From | Merges Into |
|--------|---------|--------------|-------------|
| `main` | Production-ready code | - | - |
| `develop` | Integration branch | `main` | `main` (via release) |
| `feature/*` | New features | `develop` | `develop` |
| `bugfix/*` | Bug fixes | `develop` | `develop` |
| `release/*` | Release preparation | `develop` | `main` + `develop` |
| `hotfix/*` | Emergency prod fixes | `main` | `main` + `develop` |

### Branch Naming Conventions

```
feature/phase1-database-setup
feature/phase2-jwt-authentication
feature/add-weather-widget
bugfix/fix-login-redirect
release/v1.0.0
hotfix/critical-auth-bypass
```

### Workflow

#### Starting a New Feature
```bash
# Always start from develop
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name
```

#### Completing a Feature
```bash
# Push your feature branch
git push -u origin feature/your-feature-name

# Create a Pull Request: feature/* → develop
# After PR review & approval, merge via GitHub
```

#### Creating a Release
```bash
git checkout develop
git checkout -b release/v1.0.0
# Final testing, version bumps, changelog
# PR: release/* → main (and back-merge to develop)
```

---

## Commit Message Convention

We follow **Conventional Commits**:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types
| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Formatting, no code change |
| `refactor` | Code change that neither fixes nor adds |
| `test` | Adding or updating tests |
| `chore` | Maintenance tasks |

### Examples
```
feat(auth): add JWT token generation
fix(dashboard): resolve widget drag-drop ordering
test(auth): add unit tests for login service
docs(readme): update installation instructions
```

---

## Testing Strategy (TDD-Light)

We follow **Test-Driven Development Light** - write tests alongside code, not strictly before.

### When to Write Tests

| Phase | What to Test | When |
|-------|--------------|------|
| Domain/Core | Entities, value objects, domain logic | **Immediately** with the code |
| Application | Services, use cases, DTOs | **Immediately** with the code |
| Infrastructure | Repository implementations | **After** implementation, using integration tests |
| API | Endpoints | **After** implementation, using integration tests |
| UI (Blazor) | Critical user flows | **After** feature complete, using E2E tests (optional) |

### Test Project Structure
```
tests/
├── HomeWidgets.UnitTests/           # Fast, isolated tests
│   ├── Core/                        # Domain entity tests
│   └── Application/                 # Service tests
└── HomeWidgets.IntegrationTests/    # Tests with real dependencies
    ├── Api/                         # API endpoint tests
    └── Infrastructure/              # Database tests
```

### Test Naming Convention
```csharp
// Pattern: MethodName_StateUnderTest_ExpectedBehavior
public void CreateUser_WithValidEmail_ReturnsUser()
public void CreateUser_WithInvalidEmail_ThrowsValidationException()
public void Login_WithCorrectCredentials_ReturnsJwtToken()
```

### Code Coverage Goals
- **Core/Domain**: 90%+ (critical business logic)
- **Application/Services**: 80%+
- **Infrastructure**: 60%+ (integration tests)
- **Overall**: 70%+

---

## Pull Request Process

1. **Create PR** from `feature/*` → `develop`
2. **Fill out PR template** (description, testing done, screenshots if UI)
3. **Automated checks must pass** (build, tests, linting)
4. **Code review** required before merge
5. **Squash merge** preferred for clean history

---

## Code Review Checklist

- [ ] Code follows C# coding conventions
- [ ] Unit tests added/updated
- [ ] No commented-out code
- [ ] No hardcoded secrets/connection strings
- [ ] Error handling is appropriate
- [ ] Documentation updated if needed

