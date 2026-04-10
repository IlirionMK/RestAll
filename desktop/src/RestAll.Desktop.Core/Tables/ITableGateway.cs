namespace RestAll.Desktop.Core.Tables;

public interface ITableGateway
{
    Task<List<Table>> GetTablesAsync(CancellationToken cancellationToken);
    Task<bool> UpdateTableStatusAsync(int id, TableStatus status, CancellationToken cancellationToken);
}
