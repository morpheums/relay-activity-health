namespace Relay.Api.Http;

public sealed record BaselineResponse(int WeeksUsed, double? Median, int? Low, int? High);
