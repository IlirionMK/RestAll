namespace RestAll.Desktop.Core.Orders;

public interface IManageOrdersUseCase
{
    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken);
    Task<Order?> CreateOrderAsync(int tableId, CancellationToken cancellationToken);
    Task<Order?> AddOrderItemsAsync(int orderId, List<OrderItem> items, CancellationToken cancellationToken);
    Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId, CancellationToken cancellationToken);
    Task<bool> PayOrderAsync(int orderId, CancellationToken cancellationToken);
}

public sealed class ManageOrdersUseCase : IManageOrdersUseCase
{
    private readonly IOrderGateway _gateway;

    public ManageOrdersUseCase(IOrderGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        return await _gateway.GetOrdersAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        return await _gateway.GetOrderAsync(id, cancellationToken);
    }

    public async Task<Order?> CreateOrderAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _gateway.CreateOrderAsync(tableId, cancellationToken);
    }

    public async Task<Order?> AddOrderItemsAsync(int orderId, List<OrderItem> items, CancellationToken cancellationToken)
    {
        return await _gateway.AddOrderItemsAsync(orderId, items, cancellationToken);
    }

    public async Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId, CancellationToken cancellationToken)
    {
        return await _gateway.RemoveOrderItemAsync(orderId, orderItemId, cancellationToken);
    }

    public async Task<bool> PayOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        return await _gateway.PayOrderAsync(orderId, cancellationToken);
    }
}
