using LibraryApi.Dtos;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/books")]
public class BookController(BookService bookService): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookSummaryDto>>> GetBooksAsync(
        [FromQuery] string? search,
        [FromQuery] Guid? genreId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return Problem(statusCode: 400, title: "Page and pageSize must be greater than 0 and pageSize must be less than or equal to 100.");
        }

        var result = await bookService.GetBooksAsync(search, genreId, page, pageSize, ct);
        return Ok(result);
    }   

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDetailsDto>> GetBookByIdAsync([FromRoute] Guid id, CancellationToken ct = default)
    {
        var book = await bookService.GetByIdAsync(id, ct);
        if (book is null)
        {
            return NotFound();
        }
        return Ok(book);
    }
}
