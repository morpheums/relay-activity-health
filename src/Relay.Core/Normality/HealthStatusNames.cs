namespace Relay.Core.Normality;

public static class HealthStatusNames
{
    public const string InsufficientData = "insufficient_data";
    public const string Normal = "normal";
    public const string Above = "above";
    public const string Below = "below";

    public static string ToName(this HealthStatus status) =>
        throw new NotImplementedException();
}
