namespace RestAll.Desktop.Core.Orders;

public interface IOrderGateway
{
    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken);
    Task<Order?> CreateOrderAsync(int tableId, CancellationToken cancellationToken);
    Task<Order?> AddOrderItemsAsync(int orderId, List<OrderItem> items, CancellationToken cancellationToken);
    Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId, CancellationToken cancellationToken);
    Task<bool> PayOrderAsync(int orderId, CancellationToken cancellationToken);
    Task<bool> RequestBillAsync(int orderId, CancellationToken cancellationToken);
}
