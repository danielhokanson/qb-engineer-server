# Contributing to qb-engineer-server

For project-wide guidelines (branch model, PR conventions, code style),
see the umbrella repo:
**https://github.com/danielhokanson/qb-engineer/blob/main/CONTRIBUTING.md**

## Repo-specific setup

You'll need .NET 9 SDK and Docker (for Postgres).

```bash
git clone https://github.com/danielhokanson/qb-engineer-server.git
cd qb-engineer-server

# Start a Postgres for local dev (port 5432):
docker run -d --name qb-engineer-db \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=qb_engineer \
  -p 5432:5432 \
  postgres:17

# Restore + run
dotnet restore
dotnet run --project qb-engineer.api
```

API will start at http://localhost:5000. EF migrations auto-apply on
startup.

## Tests

```bash
dotnet build                                    # analyzers run during build
dotnet test                                     # unit + integration
dotnet test --filter "Category=Unit"            # unit only (fast)
```

Integration tests use a real Postgres (the same one above is fine) — no
mocks for the database layer.

## Adding a migration

```bash
dotnet ef migrations add MyMigrationName \
  --project qb-engineer.data \
  --startup-project qb-engineer.api
```

The "host was aborted" error at the end is expected — that's just
`dotnet ef` shutting down the host after scaffolding. The migration is
created.

## Per-repo conventions

See [`docs/coding-standards.md` in the umbrella repo](https://github.com/danielhokanson/qb-engineer/blob/main/docs/coding-standards.md)
for .NET-specific patterns: MediatR handlers, FluentValidation, Fluent
API for entity configuration, no try/catch in controllers, no "DTO"
suffix.

## Where to file what

- **API endpoint bug, business logic bug, EF/migration issue** → here
- **UI rendering bug** → file in qb-engineer-ui
- **Cross-cutting design discussion** → file in qb-engineer (umbrella)
