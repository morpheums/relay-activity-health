using System.ComponentModel.DataAnnotations;

namespace Relay.Api.Http;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class ActivityTypeNameAttribute : ValidationAttribute
{
    public ActivityTypeNameAttribute()
        : base($"The field {{0}} must be one of: {string.Join(", ", ActivityTypeNames.Names)}.")
    {
    }

    public override bool IsValid(object? value) =>
        value is null || (value is string name && ActivityTypeNames.TryParse(name, out _));
}
