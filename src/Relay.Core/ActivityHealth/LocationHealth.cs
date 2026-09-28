using Relay.Core.Normality;

namespace Relay.Core.ActivityHealth;

public sealed record LocationHealth(string Location, int Count, BaselineAssessment Baseline);
