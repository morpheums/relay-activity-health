namespace Relay.Api.Composition;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddRelayApi(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddValidation();
        return services;
    }
}
