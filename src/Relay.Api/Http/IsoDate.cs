namespace Relay.Api.Http;

public readonly record struct IsoDate(DateOnly Value)
{
    public static bool TryParse(string? value, IFormatProvider? provider, out IsoDate isoDate) =>
        throw new NotImplementedException();
}
