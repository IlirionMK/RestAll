using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Offline;

public interface IOfflineStorage
{
    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<List<MenuCategory>> GetMenuCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetMenuItemsAsync(CancellationToken cancellationToken = default);
    Task SaveOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveMenuCategoriesAsync(List<MenuCategory> categories, CancellationToken cancellationToken = default);
    Task SaveMenuItemsAsync(List<MenuItem> items, CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
    Task<bool> HasDataAsync(CancellationToken cancellationToken = default);
    
    // Sync metadata methods
    Task SetSyncTimeAsync(string entityKey, DateTime syncTime, CancellationToken cancellationToken = default);
    Task<DateTime?> GetSyncTimeAsync(string entityKey, CancellationToken cancellationToken = default);
}
