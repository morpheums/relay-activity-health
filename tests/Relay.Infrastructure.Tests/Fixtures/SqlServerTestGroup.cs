namespace Relay.Infrastructure.Tests.Fixtures;

[CollectionDefinition(Name)]
public sealed class SqlServerTestGroup : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
