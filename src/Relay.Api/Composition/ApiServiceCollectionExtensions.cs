using System.Text.Json;
using System.Text.Json.Serialization;
using Relay.Api.Http;

namespace Relay.Api.Composition;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddRelayApi(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<UnhandledExceptionHandler>();
        services.AddValidation();
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)));
        return services;
    }
}
