using LibraryApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data.Seeding;

public static class DevDataSeeder
{
    // Fixed IDs so Postman requests can use them
    public static readonly Guid KarthikId     = new("5432605b-f6ef-4967-b069-28f5254c8a44");
    public static readonly Guid BobId       = new("854428e3-735e-414b-b7a0-9afa85776a15");
    public static readonly Guid LibrarianId = new("2cddddaa-8bb2-41c6-a838-44ee0fad29cc");
    private const string DevPassword = "Pass123";   // dev only

    // Used by `dotnet ef database update` and Migrate()
    public static void Seed(LibraryDbContext context)
    {
        if (!context.Books.Any())
        {
             context.Books.AddRange(BuildBooks());
        }

        if (!context.Members.Any())
        {
             context.Members.AddRange(BuildMembers());
        }

        context.SaveChanges();
    }

     // Used by MigrateAsync() / EnsureCreatedAsync()
    public static async Task SeedAsync(LibraryDbContext context, CancellationToken ct)
    {
        if (!await context.Books.AnyAsync(ct))
        {
            context.Books.AddRange(BuildBooks());
        }

        if (!await context.Members.AnyAsync(ct))
        {
            context.Members.AddRange(BuildMembers());
        }
        
        await context.SaveChangesAsync(ct);
    }

      // One place that builds the data, so both methods stay in sync
    private static List<Book> BuildBooks()
    {
        var austen   = new Author { Name = "Jane Austen" };
        var shelley  = new Author { Name = "Mary Shelley" };
        var stoker   = new Author { Name = "Bram Stoker" };
        var doyle    = new Author { Name = "Arthur Conan Doyle" };
        var pratchett = new Author { Name = "Terry Pratchett" };
        var gaiman   = new Author { Name = "Neil Gaiman" };

        return
        [
            new Book
            {
                Isbn = "9780000000001", Title = "Pride and Prejudice", PublishedYear = 1813,
                TotalCopies = 3, AvailableCopies = 3,
                BookAuthors = [new() { Author = austen, SortOrder = 0 }],
                BookGenres  = [new() { GenreId = GenreSeed.Fiction }, new() { GenreId = GenreSeed.Romance }],
            },
            new Book
            {
                Isbn = "9780000000002", Title = "Frankenstein", PublishedYear = 1818,
                TotalCopies = 2, AvailableCopies = 2,
                BookAuthors = [new() { Author = shelley, SortOrder = 0 }],
                BookGenres  = [new() { GenreId = GenreSeed.Horror }, new() { GenreId = GenreSeed.ScienceFiction }],
            },
            new Book
            {
                Isbn = "9780000000003", Title = "Dracula", PublishedYear = 1897,
                TotalCopies = 2, AvailableCopies = 2,
                BookAuthors = [new() { Author = stoker, SortOrder = 0 }],
                BookGenres  = [new() { GenreId = GenreSeed.Horror }],
            },
            new Book
            {
                Isbn = "9780000000004", Title = "The Adventures of Sherlock Holmes", PublishedYear = 1892,
                TotalCopies = 1, AvailableCopies = 1,   // only 1 copy: good for testing concurrency in Step 7
                BookAuthors = [new() { Author = doyle, SortOrder = 0 }],
                BookGenres  = [new() { GenreId = GenreSeed.Mystery }],
            },
            new Book
            {
                Isbn = "9780000000005", Title = "Good Omens", PublishedYear = 1990,
                TotalCopies = 2, AvailableCopies = 2,
                // Two authors: SortOrder keeps them in cover order
                BookAuthors = [new() { Author = pratchett, SortOrder = 0 }, new() { Author = gaiman, SortOrder = 1 }],
                BookGenres  = [new() { GenreId = GenreSeed.Fantasy }, new() { GenreId = GenreSeed.Fiction }],
            },
        ];
    }

    private static List<Member> BuildMembers()
    {
        List<Member> members =
        [
            new() { Id = KarthikId,     Email = "karthik@gmail.com",     FullName = "Karthik Reader", PasswordHash = "" },
            new() { Id = BobId,       Email = "bob@gmail.com",       FullName = "Bob Reader",   PasswordHash = "" },
            new() { Id = LibrarianId, Email = "librarian@gmail.com", FullName = "Lena Librarian", Role = MemberRole.Librarian, PasswordHash = "" },
        ];

        var hasher = new PasswordHasher<Member>();
        foreach (var m in members)
        {
            m.PasswordHash = hasher.HashPassword(m, DevPassword);
        }

        return members;
    }

}
