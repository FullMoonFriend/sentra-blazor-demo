namespace Sentra.Dashboard.Models;

/// <summary>
/// A single CIS Benchmark recommendation (e.g. "1.1.1 Enforce password history").
/// Rules are the catalog; <see cref="AppliedSetting"/> is a rule's state on one endpoint.
/// </summary>
public sealed record CisRule(
    string RuleId,
    string Title,
    string Category,
    CisLevel Level,
    string ExpectedValue);
