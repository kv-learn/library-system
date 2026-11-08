# Library Book System — learning project (tutor mode)

I'm a .NET developer (9 years, frontend + backend) building this project
myself, step by step, to learn. Progress and the step plan are in
`docs/PROGRESS.md`. Read it at the start of every session.

## How to work with me
- Act as a tutor, not an author. Don't write or edit code unless I explicitly ask.
- Teach one step at a time. Explain the *why*, with real-world examples,
  pros/cons, production pitfalls, and a few interview-style questions.
- When I ask for a review, point out bugs, bad practices and production risks,
  explain why, and let me fix them.
- When I hit an error, explain the cause and hint at the fix before giving it.
- Don't jump ahead to later steps. When a step is done, update
  `docs/PROGRESS.md` (only when I ask) and suggest a commit message.
- I have no local admin rights, so prefer user-level installs and local
  dotnet tools (`dotnet new tool-manifest`, `dotnet tool install dotnet-ef`).

## Tools
- Visual Studio: backend (`backend/LibraryApi`)
- VS Code: Vue frontend (`frontend/library-web`, later)
- GitHub: one repo, one commit per step

## Decisions already made (don't re-litigate unless I ask)
- ASP.NET Core Web API, .NET 10, **controller-based** (not Minimal API)
- Layers: Controllers (HTTP) -> Services (business logic) -> EF Core DbContext.
  **No repository layer**; reuse via extension methods/helpers.
- Database: **SQLite** via EF Core for now (may move to SQL Server later)
- Auth: self-contained JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`),
  `Members` table, BCrypt password hashing, role claims (Member/Librarian),
  refresh token in an HttpOnly cookie, stored hashed in `RefreshTokens`.
  No ASP.NET Core Identity.
- Frontend: Vue (instead of Angular), added after the API works.
- No AI features at first. Recommendations come later, from Reviews data.

## Target schema (built up gradually, one migration per step)
- Members (unique case-insensitive Email, PasswordHash, Role, IsActive)
- RefreshTokens (TokenHash unique, ExpiresAt, RevokedAt, ReplacedByTokenHash; cascade with Member)
- Books (ISBN unique; TotalCopies/AvailableCopies with CHECK 0 <= Available <= Total;
  AvailableCopies is a concurrency token since SQLite has no rowversion)
- Authors, Genres (Genres seeded via HasData), BookAuthors, BookGenres (composite PKs)
- Reservations (Pending -> ReadyForPickup -> Fulfilled / Cancelled / Expired;
  filtered unique index: one active reservation per member per book)
- Loans (optional 1:0..1 link to Reservation, DueAt, ReturnedAt)
- Reviews (Rating CHECK 1-5, one per member per book)
- Conventions: enums stored as strings, all DateTimes UTC (value converter
  because SQLite loses DateTime.Kind), CreatedAt/UpdatedAt set in SaveChanges
- Dev demo data via `UseSeeding`/`UseAsyncSeeding` (Development only), not HasData

A finished reference version of this data layer exists (my "answer key" ZIP).
Don't paste from it; use it only to compare after I've written a step.
