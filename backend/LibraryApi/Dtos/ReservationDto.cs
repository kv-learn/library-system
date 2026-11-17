using LibraryApi.Entities;

namespace LibraryApi.Dtos;

public record CreateReservationRequest(Guid BookId);

public record ReservationDto(
    Guid Id,
    Guid BookId,
    string BookTitle,
    Guid MemberId,
    ReservationStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);
