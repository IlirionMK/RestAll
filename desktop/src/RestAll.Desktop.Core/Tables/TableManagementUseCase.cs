namespace RestAll.Desktop.Core.Tables;

public interface ITableManagementUseCase
{
    Task<List<Table>> GetTablesAsync(CancellationToken cancellationToken);
    Task<bool> UpdateTableStatusAsync(int id, TableStatus status, CancellationToken cancellationToken);
}

public sealed class TableManagementUseCase : ITableManagementUseCase
{
    private readonly ITableGateway _gateway;
    private readonly RestAll.Desktop.Core.Auth.IManageProfileUseCase _profileUseCase;

    public TableManagementUseCase(ITableGateway gateway, RestAll.Desktop.Core.Auth.IManageProfileUseCase profileUseCase)
    {
        _gateway = gateway;
        _profileUseCase = profileUseCase;
    }

    public async Task<List<Table>> GetTablesAsync(CancellationToken cancellationToken)
    {
        var profile = await _profileUseCase.GetProfileAsync(cancellationToken);
        if (profile?.RestaurantId is null)
        {
            return new List<Table>();
        }

        return await _gateway.GetTablesAsync(profile.RestaurantId.Value, cancellationToken);
    }

    public async Task<bool> UpdateTableStatusAsync(int id, TableStatus status, CancellationToken cancellationToken)
    {
        return await _gateway.UpdateTableStatusAsync(id, status, cancellationToken);
    }
}
