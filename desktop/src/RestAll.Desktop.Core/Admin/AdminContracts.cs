using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.Core.Admin;

public sealed record AnalyticsRevenueStats(decimal Today, decimal ThisWeek, decimal ThisMonth);
public sealed record AnalyticsOrderStats(int Today, int ThisWeek, int ThisMonth, decimal AverageValue);
public sealed record AnalyticsTopItem(string Name, int QuantitySold, decimal Revenue);
public sealed record AnalyticsReservationStats(int Today, int ThisWeek);

public sealed record AnalyticsSummary(
    AnalyticsRevenueStats Revenue,
    AnalyticsOrderStats Orders,
    List<AnalyticsTopItem> TopItems,
    AnalyticsReservationStats Reservations
);

public sealed record AuditLogEntry(
    int Id,
    int? UserId,
    string? UserName,
    string Action,
    string? ModelType,
    int? ModelId,
    string? Payload,
    string? IpAddress,
    DateTime CreatedAt
);

public sealed record AuditLogPage(
    List<AuditLogEntry> Items,
    int CurrentPage,
    int LastPage,
    int Total,
    int PerPage
);

public sealed record AuditLogQuery(
    string? Action = null,
    int? UserId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    int PerPage = 50,
    int Page = 1
);

public interface IAdminGateway
{
    Task<List<UserProfile>> GetStaffAsync(CancellationToken cancellationToken);
    Task<UserProfile?> CreateStaffAsync(string name, string email, string password, string role, CancellationToken cancellationToken);
    Task<UserProfile?> UpdateStaffRoleAsync(int userId, string role, CancellationToken cancellationToken);
    Task<bool> DeleteStaffAsync(int userId, CancellationToken cancellationToken);
    Task<AnalyticsSummary?> GetAnalyticsSummaryAsync(CancellationToken cancellationToken);
    Task<AuditLogPage?> GetAuditLogsAsync(AuditLogQuery query, CancellationToken cancellationToken);
}

public interface IManageStaffUseCase
{
    Task<List<UserProfile>> GetStaffAsync(CancellationToken cancellationToken);
    Task<UserProfile?> CreateStaffAsync(string name, string email, string password, string role, CancellationToken cancellationToken);
    Task<UserProfile?> UpdateStaffRoleAsync(int userId, string role, CancellationToken cancellationToken);
    Task<bool> DeleteStaffAsync(int userId, CancellationToken cancellationToken);
}

public interface IGetAnalyticsSummaryUseCase
{
    Task<AnalyticsSummary> GetSummaryAsync(CancellationToken cancellationToken);
}

public interface IGetAuditLogsUseCase
{
    Task<AuditLogPage> GetLogsAsync(AuditLogQuery query, CancellationToken cancellationToken);
}

public sealed class ManageStaffUseCase : IManageStaffUseCase
{
    private readonly IAdminGateway _gateway;

    public ManageStaffUseCase(IAdminGateway gateway)
    {
        _gateway = gateway;
    }

    public Task<List<UserProfile>> GetStaffAsync(CancellationToken cancellationToken)
        => _gateway.GetStaffAsync(cancellationToken);

    public Task<UserProfile?> CreateStaffAsync(string name, string email, string password, string role, CancellationToken cancellationToken)
        => _gateway.CreateStaffAsync(name, email, password, role, cancellationToken);

    public Task<UserProfile?> UpdateStaffRoleAsync(int userId, string role, CancellationToken cancellationToken)
        => _gateway.UpdateStaffRoleAsync(userId, role, cancellationToken);

    public Task<bool> DeleteStaffAsync(int userId, CancellationToken cancellationToken)
        => _gateway.DeleteStaffAsync(userId, cancellationToken);
}

public sealed class GetAnalyticsSummaryUseCase : IGetAnalyticsSummaryUseCase
{
    private readonly IAdminGateway _gateway;

    public GetAnalyticsSummaryUseCase(IAdminGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<AnalyticsSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        return await _gateway.GetAnalyticsSummaryAsync(cancellationToken)
            ?? new AnalyticsSummary(
                new AnalyticsRevenueStats(0, 0, 0),
                new AnalyticsOrderStats(0, 0, 0, 0),
                new List<AnalyticsTopItem>(),
                new AnalyticsReservationStats(0, 0));
    }
}

public sealed class GetAuditLogsUseCase : IGetAuditLogsUseCase
{
    private readonly IAdminGateway _gateway;

    public GetAuditLogsUseCase(IAdminGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<AuditLogPage> GetLogsAsync(AuditLogQuery query, CancellationToken cancellationToken)
    {
        return await _gateway.GetAuditLogsAsync(query, cancellationToken)
            ?? new AuditLogPage(new List<AuditLogEntry>(), query.Page, query.Page, 0, query.PerPage);
    }
}

