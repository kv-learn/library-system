using LibraryApi.Authentication;
using LibraryApi.Dtos;
using LibraryApi.Entities;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[Authorize]
[Route("api/reservations")]
public class ReservationsController(ReservationService reservationService) : BaseApiController
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationDto>> GetReservation(Guid id, CancellationToken ct)
    {
        var reservation = await reservationService.GetByIdAsync(id, User.GetMemberId(), User.IsLibrarian(), ct);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [Authorize(Roles = nameof(MemberRole.Librarian))]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetAll(
            [FromQuery] ReservationStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return Problem(statusCode: 400, title: "Page must be >= 1 and pageSize between 1 and 100");

        return Ok(await reservationService.GetAllAsync(status, page, pageSize, ct));
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationRequest request, CancellationToken ct)
    {
        var result = await reservationService.ReserveAsync(request.BookId, User.GetMemberId(), ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetReservation), new { id = result.Value!.Id }, result.Value);
        }

        return ToProblem(result.Error!);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationDto>> CancelReservation(Guid id, CancellationToken ct)
    {
        var result = await reservationService.CancelAsync(id, User.GetMemberId(), User.IsLibrarian(), ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return ToProblem(result.Error!);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<ReservationDto>>> GetMine(CancellationToken ct) =>
        Ok(await reservationService.GetForMemberAsync(User.GetMemberId(), ct));

}
