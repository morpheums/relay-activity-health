using Relay.Core.Accounts;
using Relay.Core.Queries;
using Relay.Core.Tests.TestDoubles;

namespace Relay.Core.Tests.Accounts;

public sealed class AccountServiceTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ListAsyncReturnsEveryAccountIncludingOneWithNoEvents()
    {
        var accountQueries = new FakeAccountQueries()
            .WithAccount(14, "Beacon Home Security", "America/New_York")
            .WithAccount(20, "Quiet Harbor Spa", "America/Los_Angeles");
        var accountService = new AccountService(accountQueries);

        var accounts = await accountService.ListAsync(CancellationToken);

        accounts.Count.ShouldBe(2);
        accounts.ShouldContain(new AccountListItem(14, "Beacon Home Security", "America/New_York"));
        accounts.ShouldContain(new AccountListItem(20, "Quiet Harbor Spa", "America/Los_Angeles"));
    }

    [Fact]
    public async Task ListAsyncNoAccountsReturnsEmptyList()
    {
        var accountService = new AccountService(new FakeAccountQueries());

        var accounts = await accountService.ListAsync(CancellationToken);

        accounts.ShouldBeEmpty();
    }

    [Fact]
    public async Task ListAsyncOrdersByNameOrdinalSoUppercaseSortsBeforeLowercase()
    {
        var accountQueries = new FakeAccountQueries()
            .WithAccount(1, "beacon Labs", "UTC")
            .WithAccount(2, "Zenith Storage", "UTC")
            .WithAccount(3, "Acme Dental", "UTC");
        var accountService = new AccountService(accountQueries);

        var accounts = await accountService.ListAsync(CancellationToken);

        accounts.Select(account => account.Name).ShouldBe(["Acme Dental", "Zenith Storage", "beacon Labs"]);
    }

    [Fact]
    public async Task ListAsyncEqualNamesAreOrderedById()
    {
        var accountQueries = new FakeAccountQueries()
            .WithAccount(9, "Harbor Dental", "UTC")
            .WithAccount(4, "Harbor Dental", "America/Chicago")
            .WithAccount(7, "Acme Dental", "UTC");
        var accountService = new AccountService(accountQueries);

        var accounts = await accountService.ListAsync(CancellationToken);

        accounts.Select(account => account.Id).ShouldBe([7, 4, 9]);
    }
}
