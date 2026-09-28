namespace Relay.Core.Accounts;

public interface IAccountService
{
    Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken);
}
