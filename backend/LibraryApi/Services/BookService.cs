using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService(LibraryDbContext context) 
{
    public async Task<PagedResult<BookSummaryDto>> GetBooksAsync(string? search, Guid? genreId, int page, int pageSize, CancellationToken ct)
    {
        IQueryable<Book> query = context.Books;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(b => EF.Functions.Like(b.Title, pattern));
        }

        if (genreId.HasValue)
        {
            query = query.Where(b => b.BookGenres.Any(g => g.GenreId == genreId.Value));
        }

        var totalCount = await query.CountAsync(ct);

        var books = await query
            .OrderBy(b => b.Title)
            .ThenBy(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookSummaryDto(
                b.Id,
                b.Title,
                b.BookAuthors.OrderBy(ba => ba.SortOrder).Select(ba => ba.Author.Name).ToList(),
                b.BookGenres.Select(bg => bg.Genre.Name).OrderBy(n => n).ToList(),
                b.PublishedYear,
                b.AvailableCopies,
                b.TotalCopies))
            .ToListAsync(cancellationToken: ct);

        return new PagedResult<BookSummaryDto>(books, page, pageSize, totalCount);
       
    }

    public Task<BookDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
            return context.Books
                .Where(b => b.Id == id)
                .Select(b => new BookDetailsDto(
                    b.Id,
                    b.Isbn,
                    b.Title,
                    b.Description,
                    b.PublishedYear,
                    b.BookAuthors.OrderBy(ba => ba.SortOrder).Select(ba => ba.Author.Name).ToList(),
                    b.BookGenres.Select(bg => bg.Genre.Name).OrderBy(n => n).ToList(),
                    b.TotalCopies,
                    b.AvailableCopies,
                    b.Reviews.Average(r => (double?)r.Rating),
                    b.Reviews.Count))
                .FirstOrDefaultAsync(ct);
    } 
}
