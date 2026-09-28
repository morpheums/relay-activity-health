using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Relay.Infrastructure.Persistence;

public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    instant => instant.Kind == DateTimeKind.Local ? instant.ToUniversalTime() : instant,
    stored => DateTime.SpecifyKind(stored, DateTimeKind.Utc));
