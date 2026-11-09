# Progress

## Step plan
- [x] **1. Repo + empty API project**: GitHub repo `library-system`
      (VisualStudio .gitignore), ASP.NET Core Web API in `backend/LibraryApi`,
      .NET 10, controllers, OpenAPI, run once, add CLAUDE.md, commit.
- [x] **2. EF Core + Books table**: packages, DbContext, connection string,
      first migration, inspect the .db file.
- [ ] **3. Authors + Genres**: many-to-many join tables, second migration,
      read the generated migration.
- [ ] **4. Members + RefreshTokens**: unique NOCASE email, cascade delete.
- [ ] **5. Reservations, Loans, Reviews**: check constraints, filtered unique
      index, concurrency token.
- [ ] **6. Seeding**: HasData vs UseSeeding/UseAsyncSeeding.
- [ ] **7. Services + controllers**: BooksController, ReservationsController,
      reserve flow with concurrency handling.
- [ ] **8. JWT auth**: login, refresh, roles.
- [ ] **9. Vue frontend** (VS Code).

## Current status
Steps 1-2 done. Next: Step 3 (Authors + Genres).

Choices made along the way:
- Primary keys are `Guid` (stored as TEXT in SQLite). Use the same key type for every table.
- Entity config uses `IEntityTypeConfiguration<T>` classes in `Data/Configurations`,
  loaded with `ApplyConfigurationsFromAssembly`.
- `Book.PublishedYear` is optional (`int?`); `Description` is optional.
- Connection string `ConnectionStrings:DefaultConnection` = `Data Source=library.db`
  (relative to the working directory).
- `dotnet-tools.json` (dotnet-ef 10.0.14) is at the repo root; run `dotnet tool restore`.

## Notes / things I learned
<!-- add a line per step -->
- **Step 1:** keep per-developer files (`*.csproj.user`, `.vscode/`) out of git.
- **Step 2:**
  - Keep all EF package and `dotnet-ef` versions the same.
  - DbContext is Scoped (not thread-safe; it tracks every entity it loads). Long-lived
    services use `IServiceScopeFactory` or `IDbContextFactory`.
  - SQLite ignores `HasMaxLength` (it stores TEXT with no length), so validate input in the API.
  - A value type's nullability comes from the C# type (`int` vs `int?`), not `.IsRequired()`.
  - Undo a migration that hasn't been shared: `database update 0` (or the previous
    migration) → `migrations remove`. Never delete migration files by hand, because the
    snapshot gets out of sync. Once shared, only add a new migration.
  - Production: deploy with a reviewed `migrations script --idempotent` or a bundle,
    not `Migrate()` at startup. EF 9+ uses `__EFMigrationsLock` to stop concurrent runs.
