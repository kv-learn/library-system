using LibraryApi.Data;
using LibraryApi.Entities;
using LibraryApi.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace LibraryApi.Services;

public class RegistrationService(LibraryDbContext context)
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan HoldPeriod = TimeSpan.FromDays(3);

    public Task<ReservationDtos?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return context.Reservations
            .Where(r => r.Id == id)
            .Select(r => new ReservationDtos(
                r.Id,
                r.BookId,
                r.Book.Title,
                r.MemberId,
                r.Status,
                r.CreatedAt,
                r.ExpiresAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<ReservationDtos>> ReserveAync(Guid bookId, Guid memberId, CancellationToken ct)
    {
        try
        {
            return await WithConcurrencyRetryAsync(() => TryReserveAsync(bookId, memberId, ct));
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return Result<ReservationDtos>.Fail(ErrorType.Conflict, "You already have an active reservation for this book.");
        }
    }

    public Task<Result<ReservationDtos>> CancelAsync(Guid reservationId, CancellationToken ct)
        => WithConcurrencyRetryAsync(() => TryCancelAsync(reservationId, ct));


    private async Task<Result<ReservationDtos>> TryReserveAsync(Guid bookId, Guid memberId, CancellationToken ct)
    {
        var currentBook = await context.Books.FirstOrDefaultAsync(b => b.Id == bookId, ct);

        if (currentBook is null)
        {
            return Result<ReservationDtos>.Fail(ErrorType.NotFound, "Book not found.");
        }

        if (!await context.Members.AnyAsync(m => m.Id == memberId, ct))
        {
            return Result<ReservationDtos>.Fail(ErrorType.NotFound, "Member not found.");
        }

        var alreadyReserved = await context.Reservations
            .AnyAsync(r => r.BookId == bookId && r.MemberId == memberId && r.Status == ReservationStatus.Active, ct);

        if (alreadyReserved)
        {
            return Result<ReservationDtos>.Fail(ErrorType.Conflict, "You already have an active reservation for this book.");
        }

        if (currentBook.AvailableCopies <= 0)
        {
            return Result<ReservationDtos>.Fail(ErrorType.Conflict, "No available copies for this book.");
        }

        var bookReservation = new Reservation
        {
            BookId = bookId,
            MemberId = memberId,
            ExpiresAt = DateTime.UtcNow.Add(HoldPeriod)
        };

        currentBook.AvailableCopies--;
        context.Reservations.Add(bookReservation);

        await context.SaveChangesAsync(ct);

        return Result<ReservationDtos>.Ok(ToDto(bookReservation, currentBook.Title));
    }

    private async Task<Result<ReservationDtos>> TryCancelAsync(Guid reservationId, CancellationToken ct)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == reservationId, ct);

        if (reservation is null)
        {
            return Result<ReservationDtos>.Fail(ErrorType.NotFound, "Reservation not found.");
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            return Result<ReservationDtos>.Fail(ErrorType.Conflict, "Only active reservations can be cancelled.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.Book.AvailableCopies++;

        await context.SaveChangesAsync(ct);

        return Result<ReservationDtos>.Ok(ToDto(reservation, reservation.Book.Title));
    }

    private async Task<Result<T>> WithConcurrencyRetryAsync<T>(Func<Task<Result<T>>> action)
    {
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            try
            {
                return await action();
            }
            catch (DbUpdateConcurrencyException)
            {
                context.ChangeTracker.Clear(); // Clear the change tracker to avoid stale state
            }
        }

        return Result<T>.Fail(ErrorType.Conflict, "Could not complete the operation due to concurrent updates. Please try again.");
    }

    private ReservationDtos ToDto(Reservation r, string bookTitle) =>
        new(r.Id, r.BookId, bookTitle, r.MemberId, r.Status, r.CreatedAt, r.ExpiresAt);

}
