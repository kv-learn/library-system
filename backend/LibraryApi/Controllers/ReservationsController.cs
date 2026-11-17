using LibraryApi.Dtos;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;
        
[Route("api/reservations")]
public class ReservationsController(ReservationService registrationService) : BaseApiController
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationDto>> GetReservation(Guid id, CancellationToken ct)
    {
        var reservation = await registrationService.GetByIdAsync(id, ct);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationRequest request, CancellationToken ct)
    {
        var result = await registrationService.ReserveAsync(request.BookId, request.MemberId, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetReservation), new { id = result.Value!.Id }, result.Value);
        }

        return ToProblem(result.Error!);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationDto>> CancelReservation(Guid id, CancellationToken ct)
    {
        var result = await registrationService.CancelAsync(id, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return ToProblem(result.Error!);
    }

}
