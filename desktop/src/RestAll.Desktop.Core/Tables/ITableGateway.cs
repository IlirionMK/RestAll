namespace RestAll.Desktop.Core.Tables;

public interface ITableGateway
{
    Task<List<Table>> GetTablesAsync(int restaurantId, CancellationToken cancellationToken);
    Task<bool> UpdateTableStatusAsync(int id, TableStatus status, CancellationToken cancellationToken);
}
