using LibraryApi.Entities;

namespace LibraryApi.Dtos;

public record CreateReservationRequest(Guid BookId, Guid MemberId);

public record ReservationDto(
    Guid Id,
    Guid BookId,
    string BookTitle,
    Guid MemberId,
    ReservationStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);
