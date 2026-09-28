using System.Globalization;

namespace Relay.Infrastructure.Tests.Fixtures;

public static class Utc
{
    public static DateTime At(string isoInstant) =>
        DateTime.Parse(isoInstant, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
