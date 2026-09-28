namespace Relay.Api.Tests.Fixtures;

public sealed class EnvironmentVariableScope : IDisposable
{
    private readonly Dictionary<string, string?> originalValues;

    public EnvironmentVariableScope(IReadOnlyDictionary<string, string?> scopedValues)
    {
        originalValues = scopedValues.Keys.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        foreach (var (name, value) in scopedValues)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    public void Dispose()
    {
        foreach (var (name, value) in originalValues)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
