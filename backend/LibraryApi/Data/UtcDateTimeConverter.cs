using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LibraryApi.Data;

// SQLite has no time zone: stamp everything read back as UTC
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
