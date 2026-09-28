using Relay.Core.Accounts;
using Relay.Core.Queries;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Queries;

public sealed class SqlAccountQueries(RelayDbContext dbContext) : IAccountQueries
{
    private readonly RelayDbContext _dbContext = dbContext;

    public Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<AccountListItem?> FindAsync(int accountId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
