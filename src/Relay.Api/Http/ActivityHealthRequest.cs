using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Relay.Core.Queries;

namespace Relay.Api.Http;

public sealed record ActivityHealthRequest(
    [FromRoute] int AccountId,
    [FromQuery, IsoDate] string? Week,
    [FromQuery, MinLength(1), RegularExpression(ActivityTypeNames.ExactMatchPattern)] string? Type);
