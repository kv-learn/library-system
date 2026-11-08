# Progress

## Step plan
- [ ] **1. Repo + empty API project**: GitHub repo `library-system`
      (VisualStudio .gitignore), ASP.NET Core Web API in `backend/LibraryApi`,
      .NET 10, controllers, OpenAPI, run once, add CLAUDE.md, commit.
- [ ] **2. EF Core + Books table**: packages, DbContext, connection string,
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
Starting Step 1.

## Notes / things I learned
<!-- add a line per step -->
