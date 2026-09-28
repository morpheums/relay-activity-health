using Relay.Core.Normality;

namespace Relay.Core.ActivityHealth;

public sealed record AccountSummary(int Count, BaselineAssessment Baseline);
