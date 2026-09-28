namespace Relay.Api.Http;

public sealed record LocationHealthResponse(
    string Location,
    int Count,
    BaselineResponse Baseline,
    string Status,
    double? Deviation);
