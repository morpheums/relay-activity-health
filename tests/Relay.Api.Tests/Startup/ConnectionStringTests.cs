using Microsoft.Extensions.Hosting;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.Startup;

[Collection(SeededApiTestGroup.Name)]
public sealed class ConnectionStringTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task StartInDevelopmentWithoutConnectionStringFailsFastNamingTheSetting()
    {
        await using var factory = new RelayApiFactory(connectionString: null, Environments.Development);

        var startupFailure = Should.Throw<Exception>(() => factory.CreateClient().Dispose());

        MessagesOf(startupFailure).ShouldContain(
            message => message.Contains("ConnectionStrings:Relay", StringComparison.Ordinal)
                || message.Contains("ConnectionStrings__Relay", StringComparison.Ordinal));
    }

    private static List<string> MessagesOf(Exception startupFailure)
    {
        var messages = new List<string>();
        var pending = new Stack<Exception>([startupFailure]);
        while (pending.TryPop(out var exception))
        {
            messages.Add(exception.Message);
            if (exception is AggregateException aggregate)
            {
                aggregate.InnerExceptions.ToList().ForEach(pending.Push);
            }
            else if (exception.InnerException is not null)
            {
                pending.Push(exception.InnerException);
            }
        }

        return messages;
    }
}
