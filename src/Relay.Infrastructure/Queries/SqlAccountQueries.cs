using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Relay.Core.Queries;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Queries;

public sealed class SqlAccountQueries(RelayDbContext dbContext) : IAccountQueries
{
    private const string ListAccountsSql = """
        SELECT accounts.id AS Id, accounts.name AS Name, accounts.timezone AS Timezone
        FROM accounts
        """;

    private const string FindAccountSql = """
        SELECT accounts.id AS Id, accounts.name AS Name, accounts.timezone AS Timezone
        FROM accounts
        WHERE accounts.id = @accountId
        """;

    public async Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Database
            .SqlQueryRaw<AccountListItem>(ListAccountsSql)
            .ToListAsync(cancellationToken);

    public async Task<AccountListItem?> FindAsync(int accountId, CancellationToken cancellationToken)
    {
        var matchingAccounts = await dbContext.Database
            .SqlQueryRaw<AccountListItem>(FindAccountSql, new SqlParameter("@accountId", SqlDbType.Int) { Value = accountId })
            .ToListAsync(cancellationToken);
        return matchingAccounts.SingleOrDefault();
    }
}
