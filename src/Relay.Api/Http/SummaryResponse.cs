namespace Relay.Api.Http;

public sealed record SummaryResponse(int Count, BaselineResponse Baseline, string Status, double? Deviation);
