# Home Widgets

A learning-first full-stack Blazor dashboard app.

## Repo layout (in progress)
- `src/HomeWidgets.App`: Blazor Server UI host (dashboard)
- `src/HomeWidgets.Api`: Minimal API + JWT auth + data access (coming next)
- `src/HomeWidgets.Shared`: Shared DTOs/contracts between App and Api

> Note: the original starter project still exists under `HomeWidgets/` while we transition.

## Prereqs
- .NET SDK 10
- Docker Desktop (for Postgres)

## Run Postgres (dev)
```bash
docker compose up -d
```

Default dev credentials (for learning only):
- DB: `homewidgets`
- User: `homewidgets`
- Pass: `homewidgets`

## Run the API (dev)
```bash
dotnet run --project src/HomeWidgets.Api
```

## Run the App (dev)
```bash
dotnet run --project src/HomeWidgets.App
```

## Milestones checklist
- [x] Split solution into App / Api / Shared projects
- [x] Add Docker Compose for Postgres
- [ ] Add JWT auth (register/login/refresh/logout) in Api
- [ ] Protect `/dashboard` in App using JWT
- [ ] Add EF Core + migrations in Api
- [ ] Add widget registry + first real widgets
- [ ] Add unit + integration tests

