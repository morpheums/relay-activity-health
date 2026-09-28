using Microsoft.AspNetCore.Mvc;

namespace Relay.Api.Http;

public sealed record ActivityHealthRequest(
    [FromRoute] int AccountId,
    [FromQuery, IsoDate] string? Week,
    [FromQuery, ActivityTypeName] string? Type);
