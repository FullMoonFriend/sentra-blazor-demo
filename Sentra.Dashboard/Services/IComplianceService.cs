using Sentra.Dashboard.Models;

namespace Sentra.Dashboard.Services;

/// <summary>
/// Data access for fleet compliance state. The UI depends only on this interface,
/// so the in-memory demo store can be swapped for an EF Core / API-backed
/// implementation without touching any component.
/// </summary>
public interface IComplianceService
{
    Task<IReadOnlyList<EndpointDevice>> GetEndpointsAsync(CancellationToken ct = default);
    Task<EndpointDevice?> GetEndpointAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DriftEvent>> GetDriftEventsAsync(int? take = null, CancellationToken ct = default);
    Task<IReadOnlyList<TrendPoint>> GetComplianceTrendAsync(int days = 30, CancellationToken ct = default);
    Task<FleetSummary> GetFleetSummaryAsync(CancellationToken ct = default);

    /// <summary>Re-enforce a single drifted setting on an endpoint.</summary>
    Task RemediateAsync(Guid endpointId, string ruleId, CancellationToken ct = default);

    /// <summary>Re-enforce every drifted setting on an endpoint. Returns the number remediated.</summary>
    Task<int> RemediateAllAsync(Guid endpointId, CancellationToken ct = default);
}
