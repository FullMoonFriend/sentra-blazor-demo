namespace Sentra.Dashboard.Models;

/// <summary>A detected deviation from the enforced baseline on an endpoint.</summary>
public sealed class DriftEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid EndpointId { get; init; }
    public required string Hostname { get; init; }
    public required string ClientName { get; init; }
    public required string RuleId { get; init; }
    public required string RuleTitle { get; init; }
    public required string ExpectedValue { get; init; }
    public required string ObservedValue { get; init; }
    public DateTime DetectedAt { get; init; }
    public DriftStatus Status { get; set; } = DriftStatus.Open;
}
