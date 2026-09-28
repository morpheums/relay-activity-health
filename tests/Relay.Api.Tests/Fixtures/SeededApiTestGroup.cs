namespace Relay.Api.Tests.Fixtures;

[CollectionDefinition(Name)]
public sealed class SeededApiTestGroup : ICollectionFixture<SeededApiFixture>
{
    public const string Name = "SeededApi";
}
