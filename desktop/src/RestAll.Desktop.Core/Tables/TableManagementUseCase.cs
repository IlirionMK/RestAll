namespace RestAll.Desktop.Core.Tables;

public interface ITableManagementUseCase
{
    Task<List<Table>> GetTablesAsync(CancellationToken cancellationToken);
    Task<bool> UpdateTableStatusAsync(int id, TableStatus status, CancellationToken cancellationToken);
}

public sealed class TableManagementUseCase : ITableManagementUseCase
{
    private readonly ITableGateway _gateway;

    public TableManagementUseCase(ITableGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<List<Table>> GetTablesAsync(CancellationToken cancellationToken)
    {
        return await _gateway.GetTablesAsync(cancellationToken);
    }

    public async Task<bool> UpdateTableStatusAsync(int id, TableStatus status, CancellationToken cancellationToken)
    {
        return await _gateway.UpdateTableStatusAsync(id, status, cancellationToken);
    }
}
