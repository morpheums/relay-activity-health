using Relay.Core.Accounts;

namespace Relay.Core.Queries;

public interface IAccountQueries
{
    Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken);

    Task<AccountListItem?> FindAsync(int accountId, CancellationToken cancellationToken);
}
