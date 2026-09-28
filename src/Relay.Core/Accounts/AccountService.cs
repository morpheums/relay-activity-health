using Relay.Core.Queries;

namespace Relay.Core.Accounts;

public sealed class AccountService(IAccountQueries accountQueries) : IAccountService
{
    public async Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken)
    {
        var accounts = await accountQueries.ListAsync(cancellationToken);
        return accounts
            .OrderBy(account => account.Name, StringComparer.Ordinal)
            .ThenBy(account => account.Id)
            .ToList();
    }
}
