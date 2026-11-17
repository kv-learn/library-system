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
- [x] **7. Services + controllers**: BooksController, ReservationsController,
      reserve flow with concurrency handling.
- [ ] **8. JWT auth**: login, refresh, roles.
  - [x] 8A: JWT setup, register, login, `GET /api/auth/me`.
  - [x] 8B: protect Reservations (member id from the token, ownership, Librarian role).
  - [ ] 8C: refresh tokens (rotation, reuse detection), logout.
- [ ] **9. Vue frontend** (VS Code).

## Current status
Steps 1-7, 8A and 8B done. Next: 8C (refresh tokens, logout).

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
  holds auth code (`JwtOptions`, `AuthClaims`, `TokenService`).
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
  - Dev members (Karthik, Bob = Member; Lena = Librarian) seeded in Step 7 with fixed IDs
    and `PasswordHasher<Member>` (password `Pass123`, dev only).
  - Seeding runs on `dotnet ef database update` (sync `UseSeeding`); `dotnet run` doesn't
    seed because nothing calls `Migrate()` at startup.
  - "Sherlock Holmes" has 1 copy on purpose, to test concurrency in Step 7.
- API layer (Step 7):
  - Layers: `Controllers/` (HTTP only) → `Services/` (EF + rules) → DbContext. No
    interfaces until a second implementation or a mock is needed. Services are Scoped.
  - Never return entities: DTOs are positional records in `Dtos/`, filled with `Select`
    projection (no `Include`, no tracking, only needed columns, one query).
  - Lists are paged: `PagedResult<T>`, `page >= 1`, `pageSize` 1-100, stable order
    (`OrderBy(Title).ThenBy(Id)`). Search uses `EF.Functions.Like` (query key `search`).
  - Services return `Result<T>` (`Ok` / `Fail(ErrorType, message)`, private ctor) for
    expected outcomes; controllers map `NotFound` → 404, `Conflict` → 409 as ProblemDetails.
  - Reserve: check book/member/existing Active/copies → `AvailableCopies--` + insert in one
    `SaveChanges`. `DbUpdateConcurrencyException` → `ChangeTracker.Clear()` and retry
    (max 3). Unique-index violation (SQLite 2067) → 409 as a safety net. Hold = 3 days.
  - Cancel is `POST /api/reservations/{id}/cancel` (status change, row kept), Active only.
  - The member id comes from the JWT (`sub`) since Step 8B, never from the request body.
  - Enums are serialized as strings (`JsonStringEnumConverter`).
  - All `DateTime`s are UTC: `UtcDateTimeConverter` applied by convention in
    `ConfigureConventions`.
  - Manual testing: Postman collection (kept outside the repo) and curl.
- Auth (Step 8A):
  - JWT bearer, HS256, access token 15 min. Settings in the `Jwt` section of
    `appsettings.json`; the signing key (`Jwt:SigningKey`, 64 random bytes) is in
    **user secrets** in dev (`Jwt__SigningKey` env var in production), never in git.
  - `JwtOptions` is validated at startup (`ValidateDataAnnotations().ValidateOnStart()`).
  - Claims: `sub` (member id), `email`, `name`, `role`, `jti`. `MapInboundClaims = false`
    keeps those short names; `NameClaimType`/`RoleClaimType` point at them so
    `[Authorize(Roles = ...)]` works. `ClockSkew` 30 s (default is 5 min).
  - `TokenService` is a singleton (config only); `AuthService` is scoped (DbContext).
    `IPasswordHasher<Member>` = `PasswordHasher<Member>` (PBKDF2), singleton.
  - Endpoints: `POST /api/auth/register`, `POST /api/auth/login` (both return
    `AuthResponse`), `[Authorize] GET /api/auth/me`.
  - Emails are trimmed + lower-cased before lookup/insert. Register never takes `Role`
    from the request (always `Member`); password 8-128 chars (seeded `Pass123` still logs in).
  - Login failure gives the same 401 "Invalid email or password." for unknown email and
    wrong password. `SuccessRehashNeeded` → hash upgraded on login.
  - `ErrorType.Unauthorized` → 401. `BaseApiController` (`[ApiController]` + `ToProblem`)
    is the base for all controllers.
- Authorization (Step 8B):
  - **Secure by default:** fallback policy `RequireAuthenticatedUser()`; public endpoints
    opt out with `[AllowAnonymous]` (BookController, register, login, `MapOpenApi()`).
  - `ClaimsPrincipalExtensions`: `User.GetMemberId()` (reads `sub`, throws if missing,
    only used on `[Authorize]` endpoints) and `User.IsLibrarian()`.
  - Reservations: `[Authorize]` on the controller. `CreateReservationRequest` has only
    `BookId`; the member comes from the token.
  - Ownership: get/cancel filter `r.Id == id && (isLibrarian || r.MemberId == requesterId)`
    in the query. Not yours → **404** (don't reveal it exists), wrong role → **403**,
    no/invalid token → **401**.
  - `GET /api/reservations/mine` (own list, newest first) and
    `[Authorize(Roles = nameof(MemberRole.Librarian))] GET /api/reservations`
    (paged, optional `status` filter). Librarians can view/cancel any reservation.
  - One `Expression<Func<Reservation, ReservationDto>>` projection shared by the queries.
  - `GET /api/auth/me`: `AuthService.GetMemberAsync` returns `Result<MemberDto>`
    (member deleted → 401 ProblemDetails).
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
- **Step 7:**
  - Register every service (`AddScoped<T>()`): a missing registration builds fine and fails
    at the first request ("Unable to resolve service").
  - Route templates: `[...]` are tokens (`[controller]`), so `[Route("[api/books]")]` breaks.
  - Model binding matches query keys to parameter names and silently ignores unknown keys
    (`?search=` vs `searchTerm` → filter never applied). Check the SQL log for the `WHERE`.
  - `string.Contains` → `instr()` (case-sensitive) in SQLite; `EF.Functions.Like` isn't.
  - `Average` over no rows is SQL `NULL`: project as `(double?)` or materialization throws.
  - ASP.NET strips the `Async` suffix from action names → `CreatedAtAction(nameof(XAsync))`
    finds no route. Don't use `Async` on action names.
  - Concurrency token in practice: EF adds `WHERE AvailableCopies = @original`; 0 rows →
    whole `SaveChanges` transaction rolls back (no orphan reservation). Without the token
    two requests both write `1 - 1 = 0`: no error, no check-constraint violation, one copy
    given to two people.
  - Before retrying, `ChangeTracker.Clear()`, or the retry reuses the stale tracked entity.
  - `DbUpdateConcurrencyException` derives from `DbUpdateException`: catch the specific one
    first.
  - SQLite returns `DateTime` as `Kind = Unspecified` → JSON without `Z` → browsers treat it
    as local time. Fixed with a value converter (no migration;
    `dotnet ef migrations has-pending-model-changes` confirms).
  - Seed code only runs against an empty table: changing `BuildMembers()` doesn't update
    existing rows; update them directly or recreate the DB.
  - Debugging: F5 (C# Dev Kit) + breakpoints, Variables panel, Debug Console; or attach to a
    running `dotnet run` process. SQL logging via
    `Microsoft.EntityFrameworkCore.Database.Command: Information`.
  - OpenAPI from .NET 10 is 3.1; Postman's link import can't reach localhost → import a file
    or a Postman collection.
- **Step 8A:**
  - A JWT is signed, not encrypted: anyone can base64-decode the payload. Never put secrets
    in it. The signature only proves who issued it and that it wasn't changed.
  - `UseAuthentication()` (who are you?) must come before `UseAuthorization()` (are you
    allowed?).
  - The options binder matches property names to config keys: a typo (`SigninKey`) binds
    nothing, and `ValidateOnStart` catches it at startup instead of at the first login.
    `MinLength` vs `MaxLength` matters for a key.
  - `openssl rand -base64 64` wraps lines; pipe through `tr -d '\n'` before storing.
  - `[ApiController]` gives JSON body binding and automatic 400s. Put it on the base
    controller so every controller gets it.
  - An action without `[HttpGet("me")]` maps to the controller route for every verb;
    without `[Authorize]` it isn't protected (the middleware must reject it with 401 and
    `WWW-Authenticate`, not the action code).
  - Don't accept `Role` in register (mass assignment).
  - Decode a token: `echo $TOKEN | cut -d. -f2` then base64url-decode (add `=` padding).
- **Step 8B:**
  - Never let the body say *who* is acting: take the user id from the token. Extra JSON
    fields (`memberId`) are silently ignored once the DTO doesn't have them.
  - Fallback policy = forgetting `[AllowAnonymous]` gives a visible 401; forgetting
    `[Authorize]` without it would leak data. `MapOpenApi()` needs `.AllowAnonymous()` too.
  - 401 = who are you? 403 = I know you, not allowed. For other people's data return 404.
  - Put the ownership rule in the query `Where` (one query, can't forget a check later).
  - An `Expression<Func<...>>` (not `Func`) keeps a shared projection translatable to SQL.
  - `Result<T>` is created in services and turned into HTTP only by `ToProblem`; a
    controller can't return a `Result` as an `ActionResult`.
  - Stateless JWT: a deleted member's token stays valid until it expires; only endpoints
    that look the member up notice. Keep access tokens short.
  - `[Authorize(Roles = ...)]` needs a const: `nameof(MemberRole.Librarian)` works.
