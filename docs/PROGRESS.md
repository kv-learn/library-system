# Progress

## Step plan
- [x] **1. Repo + empty API project**: GitHub repo `library-system`
      (VisualStudio .gitignore), ASP.NET Core Web API in `backend/LibraryApi`,
      .NET 10, controllers, OpenAPI, run once, add CLAUDE.md, commit.
- [x] **2. EF Core + Books table**: packages, DbContext, connection string,
      first migration, inspect the .db file.
- [x] **3. Authors + Genres**: many-to-many join tables, second migration,
      read the generated migration.
- [x] **4. Members + RefreshTokens**: unique NOCASE email, cascade delete.
- [x] **5. Reservations, Loans, Reviews**: check constraints, filtered unique
      index, concurrency token.
- [x] **6. Seeding**: HasData vs UseSeeding/UseAsyncSeeding.
- [ ] **7. Services + controllers**: BooksController, ReservationsController,
      reserve flow with concurrency handling.
- [ ] **8. JWT auth**: login, refresh, roles.
- [ ] **9. Vue frontend** (VS Code).

## Current status
Steps 1-6 done. Next: Step 7 (Services + controllers).

Choices made along the way:
- Primary keys are `Guid` (stored as TEXT in SQLite). Use the same key type for every table.
- Entity config uses `IEntityTypeConfiguration<T>` classes in `Data/Configurations`,
  loaded with `ApplyConfigurationsFromAssembly`.
- `Book.PublishedYear` is optional (`int?`); `Description` is optional.
- Connection string `ConnectionStrings:DefaultConnection` = `Data Source=library.db`
  (relative to the working directory).
- `dotnet-tools.json` (dotnet-ef 10.0.14) is at the repo root; run `dotnet tool restore`.
- Many-to-many uses explicit join entities (`BookAuthor`, `BookGenre`) with composite
  PKs `(BookId, XId)`, not skip navigations. `BookAuthor.SortOrder` gives the author order.
- Delete behavior: **cascade from the owner** (Book → its link rows), **restrict from
  shared reference data** (Author/Genre → link rows).
- All EF entities live in `Entities/` (incl. `Member`, `RefreshToken`). `Authentication/`
  is reserved for auth code in Step 8 (token service, JWT options, password hashing).
- `Member.Email` uses `COLLATE NOCASE` on the column (the unique index inherits it).
- `Member.Role` is an enum stored as a string (`HasConversion<string>()`). The
  lowest-privilege value (`Member`) comes first, so `default(MemberRole)` is safe.
- Timestamps are UTC `DateTime` (not `DateTimeOffset`: EF's SQLite provider can't
  compare/order it in SQL).
- `CreatedAt` is set automatically: entities implement `IHasCreatedAt`, and
  `LibraryDbContext` overrides `SaveChanges(bool)` / `SaveChangesAsync(bool, ct)` to stamp
  `Added` entries with `DateTime.UtcNow`. `ExpiresAt` is business logic (set by the token
  service).
- Refresh tokens store only a SHA-256 hash (`TokenHash`, unique), never the raw token.
  `IsRevoked` is computed from `RevokedAt` (one source of truth, no column).
  Member → RefreshTokens is cascade delete.
- Reservations / Loans / Reviews (all `IHasCreatedAt`; `Loan.CreatedAt` = borrowed at):
  - `ReservationStatus` (Active, Fulfilled, Cancelled, Expired) stored as a string;
    `Reservation.ExpiresAt` = pickup deadline.
  - One **Active** reservation per member+book (unique index filtered on
    `"Status" = 'Active'`); one **open** loan per member+book (filtered on
    `"ReturnedAt" IS NULL`); one review per member+book (plain unique index).
  - Check constraints: `Rating BETWEEN 1 AND 5`; `DueAt > CreatedAt`;
    `ReturnedAt IS NULL OR ReturnedAt >= CreatedAt`; on Books
    `TotalCopies >= 0` and `0 <= AvailableCopies <= TotalCopies`.
  - Delete behavior: **Book → all three: Restrict** (no deleting books with history).
    **Member → Reservations/Reviews: Cascade** (owned by the member);
    **Member → Loans: Restrict** (lending record).
  - `Book.AvailableCopies` is the concurrency token (`IsConcurrencyToken()`): two people
    reserving the last copy → second `SaveChanges` throws `DbUpdateConcurrencyException`
    (handled in Step 7). Chosen over a `Version` column so editing a title doesn't conflict.
- Seeding (code in `Data/Seeding/`):
  - **Genres → `HasData`** (reference data, part of migrations). Fixed GUIDs live in
    `GenreSeed` so the dev seeder can link books to them.
  - **Authors + Books (+ link rows) → `UseSeeding`/`UseAsyncSeeding`**, Development only,
    via `DevDataSeeder` (skips if any book exists). Both methods share `BuildBooks()`.
  - Members are seeded in Step 8 (needs password hashing).
  - Seeding runs on `dotnet ef database update` (sync `UseSeeding`); `dotnet run` doesn't
    seed because nothing calls `Migrate()` at startup.
  - "Sherlock Holmes" has 1 copy on purpose, to test concurrency in Step 7.
- Tables are plural: entities you query directly get a `DbSet` (Books, Authors, Genres);
  join entities use `ToTable(...)` (BookAuthors, BookGenres).
- `Author.Name` is a single display name and **not** unique (different people can share
  a name). `Genre.Name` is unique (case-sensitive for now).
- `required` for scalar properties you must set; `= null!` only for reference
  navigations that EF fills in.

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
- **Step 3:**
  - EF discovers entities through navigations, even without a DbSet. Every entity needs a
    key, and EF won't guess a composite key: configure it with `HasKey(x => new { ... })`.
  - A type with no DbSet gets its table named after the class (singular) unless you use `ToTable`.
  - Always name the inverse in `WithMany(b => b.X)`; leaving it out creates a duplicate
    FK (`BookId1`).
  - EF indexes every FK unless an existing index already starts with that column. PK
    `(BookId, AuthorId)` covers BookId, so EF adds `IX_BookAuthors_AuthorId` (like a phone
    book sorted by last name can't find all "John"s).
  - Migrations create principal tables before dependents; `Down()` drops in reverse.
  - Check renames in migrations: `RenameColumn` keeps data; `DropColumn` + `AddColumn`
    loses it.
  - Fix mistakes before sharing by merging migrations: applied → `database update
    <previous>` first, then `migrations remove`; not applied → just `migrations remove`.
  - SQLite only enforces FKs when the connection runs `PRAGMA foreign_keys = ON`. EF does
    this; the `sqlite3` CLI doesn't by default.
  - Avoid SQL keywords as column names (`Order` → `SortOrder`).
  - Tools: SQLite Viewer / SQLite extensions in VS Code; `.editorconfig` +
    `dotnet format` keep formatting the same across editors.
- **Step 4:**
  - Collation belongs on the column (`UseCollation("NOCASE")`): the unique index and
    every `WHERE Email = ...` then ignore case, so no `ToLower()` needed in queries.
  - SQLite `NOCASE` only folds ASCII letters (`É` ≠ `é`). Fine for emails; still trim input.
  - Store enums as strings: readable rows, and reordering the enum can't change data.
  - Hash refresh tokens like passwords: a leaked DB then can't be used to log in.
  - Don't store a fact twice (`IsRevoked` + `RevokedAt` can disagree); compute one from
    the other. EF doesn't map get-only properties.
  - When overriding, call the **same** overload on `base`: `base.SaveChanges()` from inside
    `SaveChanges(bool)` calls back into the override → infinite recursion → crash.
  - `.IsRequired()` on a non-nullable value type (`DateTime`, enum) does nothing.
  - A clean build doesn't mean a correct model: always read the generated migration
    (it showed `Role` as INTEGER and a missing configuration).
  - Unapplied migration wrong? Just `migrations remove` and add it again.
- **Step 5:**
  - Check constraints: `builder.ToTable(t => t.HasCheckConstraint("CK_...", "sql"))`.
    The SQL is raw and provider-specific; quote column names.
  - Filtered unique index = uniqueness only for rows matching a condition
    (`.HasFilter(...)`). The filter compares stored values, so with enums-as-strings it's
    `'Active'`, not `0`.
  - A composite index `(MemberId, BookId)` can't serve `WHERE BookId = ...`, so EF still
    adds `IX_*_BookId` for the FK.
  - Concurrency token: EF adds `WHERE AvailableCopies = @original` to the UPDATE; 0 rows
    affected → exception. No SQL in the migration, only the snapshot changes.
  - SQLite can't `ALTER TABLE ADD CONSTRAINT`: the C# migration says `AddCheckConstraint`,
    but the generated SQL **rebuilds the table** (`ef_temp_Books` → copy → drop → rename,
    with `PRAGMA foreign_keys = 0` so dropping Books doesn't cascade). See it with
    `dotnet ef migrations script <from> <to>`. Existing rows that break the new rule make
    the copy fail, so check the data first.
  - Delete-behavior rule per FK, not per file: think "who owns this row?" for each
    `HasOne` separately.
  - Dates are TEXT in SQLite; EF's format (`yyyy-MM-dd HH:mm:ss.FFFFFFF`) sorts as text,
    so date check constraints compare correctly.
- **Step 6:**
  - `HasData` = seed data in the model → `InsertData`/`UpdateData`/`DeleteData` in
    migrations. Needs fixed keys and values that never change between builds (no
    `Guid.NewGuid()`/`DateTime.UtcNow`, or every `migrations add` sees a change). Only FKs,
    no navigations. Skips `SaveChanges`, so no automatic `CreatedAt`.
  - `UseSeeding` / `UseAsyncSeeding` (EF 9+) = normal C# after migrate/`EnsureCreated`.
    Full object graphs, generated keys. Runs **every** time, so it must check first
    (idempotent). Provide both: the tools call the sync one, `MigrateAsync` the async one.
  - Add only the root (`Books.AddRange`): EF follows navigations and inserts authors and
    link rows in FK order.
  - Missing `using Microsoft.EntityFrameworkCore;` → EF's `AnyAsync`/`ToListAsync` aren't
    found; in .NET 10 the compiler picks `System.Linq.AsyncEnumerable.AnyAsync` instead and
    gives a confusing "type arguments cannot be inferred" error.
  - Check the counts after seeding, e.g. `sqlite3 library.db "select count(*) from Books"`.
