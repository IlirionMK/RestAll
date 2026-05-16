using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Offline;

// Record for pending order items
public sealed record PendingOrderItem(int MenuItemId, string Name, decimal Price, int Quantity, string? Comment, string Operation);

// Record for pending order
public sealed record PendingOrder(int LocalId, int? ServerId, int TableId, int? ReservationId, string Status, DateTime CreatedAt);

// Record for pending payment
public sealed record PendingPayment(int Id, int OrderServerId, decimal Amount, string? PaymentMethod, string Status);

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
    
    // Pending orders methods (offline drafts)
    Task<int> CreatePendingOrderAsync(int tableId, int? reservationId, CancellationToken cancellationToken = default);
    Task<List<PendingOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task<PendingOrder?> GetPendingOrderAsync(int localId, CancellationToken cancellationToken = default);
    Task MarkOrderSyncedAsync(int localId, int serverId, CancellationToken cancellationToken = default);
    Task MarkOrderFailedAsync(int localId, string errorMessage, CancellationToken cancellationToken = default);
    Task MarkOrderSyncingAsync(int localId, CancellationToken cancellationToken = default);
    
    // Pending order items methods
    Task AddPendingOrderItemAsync(int localOrderId, PendingOrderItem item, CancellationToken cancellationToken = default);
    Task<List<PendingOrderItem>> GetPendingOrderItemsAsync(int localOrderId, CancellationToken cancellationToken = default);
    Task MarkPendingOrderItemSyncedAsync(int itemId, CancellationToken cancellationToken = default);
    Task RemovePendingOrderItemAsync(int localOrderId, int menuItemId, CancellationToken cancellationToken = default);
    
    // Pending payments methods
    Task AddPendingPaymentAsync(int orderServerId, decimal amount, string? paymentMethod, CancellationToken cancellationToken = default);
    Task<List<PendingPayment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
    Task MarkPaymentSyncedAsync(int paymentId, CancellationToken cancellationToken = default);
    Task MarkPaymentFailedAsync(int paymentId, string errorMessage, CancellationToken cancellationToken = default);
    
    // Count pending operations
    Task<int> GetPendingOperationsCountAsync(CancellationToken cancellationToken = default);
}
