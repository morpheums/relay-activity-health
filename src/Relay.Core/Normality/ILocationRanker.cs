using Relay.Core.ActivityHealth;

namespace Relay.Core.Normality;

public interface ILocationRanker
{
    IReadOnlyList<LocationHealth> Rank(IReadOnlyList<LocationHealth> locations);
}
