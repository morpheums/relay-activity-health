using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Relay.Api.Http;

public sealed record ActivityHealthRequest(
    [FromRoute] int AccountId,
    [FromQuery] IsoDate? Week,
    [FromQuery, RegularExpression(ActivityHealthRequest.EventTypePattern)] string? Type)
{
    public const string EventTypePattern = "^(all|call_received|lead_created|appointment_set)$";
}
