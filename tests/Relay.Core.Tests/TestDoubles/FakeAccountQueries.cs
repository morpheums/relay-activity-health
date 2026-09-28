using Relay.Core.Queries;

namespace Relay.Core.Tests.TestDoubles;

internal sealed class FakeAccountQueries : IAccountQueries
{
    private readonly List<AccountListItem> _accounts = [];

    public FakeAccountQueries WithAccount(int id, string name, string timezone)
    {
        _accounts.Add(new AccountListItem(id, name, timezone));
        return this;
    }

    public Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AccountListItem>>(_accounts.ToList());

    public Task<AccountListItem?> FindAsync(int accountId, CancellationToken cancellationToken) =>
        Task.FromResult(_accounts.SingleOrDefault(account => account.Id == accountId));
}
