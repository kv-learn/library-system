namespace LibraryApi.Dtos;

public record BookSummaryDto(
    Guid Id,
    string Title,
    List<string> Authors,
    List<string> Genres,
    int? PublishedYear,
    int AvailableCopies,
    int TotalCopies);

public record BookDetailsDto(
    Guid Id,
    string Isbn,
    string Title,
    string? Description,
    int? PublishedYear,
    List<string> Authors,
    List<string> Genres,
    int TotalCopies,
    int AvailableCopies,
    double? AverageRating,   // null when there are no reviews
    int ReviewCount);
