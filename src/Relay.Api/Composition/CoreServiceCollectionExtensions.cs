using Microsoft.Extensions.Options;
using Relay.Core.Accounts;
using Relay.Core.ActivityHealth;
using Relay.Core.Calendar;
using Relay.Core.Normality;

namespace Relay.Api.Composition;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddRelayCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NormalityOptions>()
            .Bind(configuration.GetSection(NormalityOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => options.MinimumEligibleWeeks <= options.BaselineWeeks,
                "Normality:MinimumEligibleWeeks must not exceed Normality:BaselineWeeks.")
            .ValidateOnStart();
        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<NormalityOptions>>().Value);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IWeekCalendar, WeekCalendar>();
        services.AddSingleton<IWeeklyGridBuilder, WeeklyGridBuilder>();
        services.AddSingleton<IBaselineEvaluator, BaselineEvaluator>();
        services.AddSingleton<ILocationRanker, LocationRanker>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IActivityHealthService, ActivityHealthService>();
        return services;
    }
}
