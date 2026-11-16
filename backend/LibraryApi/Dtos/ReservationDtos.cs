using LibraryApi.Entities;

namespace LibraryApi.Dtos;

public record CreateReservationRequest(Guid BookId, Guid MemberId);

public record ReservationDtos(
    Guid Id,
    Guid BookId,
    string BookTitle,
    Guid MemberId,
    ReservationStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);
