using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Offline;

public interface ISyncManager
{
    void Start();
    void Stop();

    // Enqueue create order operation. Returns a locally-created Order (temporary id) to show in UI.
    Task<Order> EnqueueCreateOrderAsync(int tableId, CancellationToken cancellationToken = default);
    
    // Enqueue add items operation for offline orders
    Task EnqueueAddItemsAsync(int localOrderId, List<OrderItem> items, CancellationToken cancellationToken = default);
    
    // Enqueue remove item operation for offline orders
    Task EnqueueRemoveItemAsync(int localOrderId, int orderItemId, CancellationToken cancellationToken = default);
    
    // Enqueue pay order operation
    Task EnqueuePayOrderAsync(int serverOrderId, CancellationToken cancellationToken = default);
}

