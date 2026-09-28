using System.Text.Json;
using System.Text.Json.Serialization;

namespace Relay.Api.Composition;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddRelayApi(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddValidation();
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)));
        return services;
    }
}
