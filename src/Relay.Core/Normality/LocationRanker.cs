using Relay.Core.ActivityHealth;

namespace Relay.Core.Normality;

public sealed class LocationRanker : ILocationRanker
{
    public IReadOnlyList<LocationHealth> Rank(IReadOnlyList<LocationHealth> locations) =>
        throw new NotImplementedException();
}
