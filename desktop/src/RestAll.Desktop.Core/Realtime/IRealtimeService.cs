using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Realtime;

public interface IRealtimeService
{
    event EventHandler<OrderUpdatedEventArgs>? OrderUpdated;
    event EventHandler<OrderItemStatusUpdatedEventArgs>? OrderItemStatusUpdated;
    event EventHandler<KitchenTicketUpdatedEventArgs>? KitchenTicketUpdated;
    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}

public class OrderUpdatedEventArgs : EventArgs
{
    public int OrderId { get; }
    public Order Order { get; }

    public OrderUpdatedEventArgs(int orderId, Order order)
    {
        OrderId = orderId;
        Order = order;
    }
}

public class OrderItemStatusUpdatedEventArgs : EventArgs
{
    public int OrderId { get; }
    public int OrderItemId { get; }
    public OrderItemStatus Status { get; }

    public OrderItemStatusUpdatedEventArgs(int orderId, int orderItemId, OrderItemStatus status)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
        Status = status;
    }
}

public class KitchenTicketUpdatedEventArgs : EventArgs
{
    public int OrderItemId { get; }
    public OrderItemStatus Status { get; }

    public KitchenTicketUpdatedEventArgs(int orderItemId, OrderItemStatus status)
    {
        OrderItemId = orderItemId;
        Status = status;
    }
}
