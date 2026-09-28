using Relay.Core.Queries;

namespace Relay.Core.Accounts;

public sealed class AccountService(IAccountQueries accountQueries) : IAccountService
{
    private readonly IAccountQueries _accountQueries = accountQueries;

    public Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
