using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Relay.Api.Http;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class IsoDateAttribute : ValidationAttribute
{
    public const string Format = "yyyy-MM-dd";

    public IsoDateAttribute()
        : base($"The field {{0}} must be a date in {Format} format.")
    {
    }

    public override bool IsValid(object? value) =>
        value is null
        || (value is string text
            && DateOnly.TryParseExact(text, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
}
