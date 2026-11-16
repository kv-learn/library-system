using LibraryApi.Dtos;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController(RegistrationService registrationService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationDtos>> GetReservation(Guid id, CancellationToken ct)
    {
        var reservation = await registrationService.GetByIdAsync(id, ct);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDtos>> CreateReservation(CreateReservationRequest request, CancellationToken ct)
    {
        var result = await registrationService.ReserveAync(request.BookId, request.MemberId, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetReservation), new { id = result.Value!.Id }, result.Value);
        }

        return ToProblem(result.Error!);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationDtos>> CancelReservation(Guid id, CancellationToken ct)
    {
        var result = await registrationService.CancelAsync(id, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return ToProblem(result.Error!);
    }

    private ObjectResult ToProblem(Error error)
    => Problem(statusCode: error.Type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    },
       title: error.Message);

}
