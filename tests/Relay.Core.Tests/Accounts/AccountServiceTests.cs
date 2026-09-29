using Relay.Core.Accounts;
using Relay.Core.Tests.TestDoubles;

namespace Relay.Core.Tests.Accounts;

public sealed class AccountServiceTests
{
    [Fact]
    public async Task ListAsyncOrdersByNameOrdinalThenById()
    {
        var accountQueries = new FakeAccountQueries()
            .WithAccount(9, "Harbor Dental", "UTC")
            .WithAccount(1, "beacon Labs", "UTC")
            .WithAccount(4, "Harbor Dental", "America/Chicago")
            .WithAccount(2, "Zenith Storage", "UTC")
            .WithAccount(7, "Acme Dental", "UTC");
        var accountService = new AccountService(accountQueries);

        var accounts = await accountService.ListAsync(TestContext.Current.CancellationToken);

        accounts.Select(account => account.Id).ShouldBe([7, 4, 9, 2, 1]);
    }
}
