using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Kitchen;

namespace RestAll.Desktop.Core.Realtime;

public interface IRealtimeService
{
    event EventHandler<OrderBillingRequestedEventArgs>? OrderBillingRequested;
    event EventHandler<KitchenOrderItemsAddedEventArgs>? KitchenOrderItemsAdded;
    event EventHandler<KitchenTicketStatusUpdatedEventArgs>? KitchenTicketStatusUpdated;
    event EventHandler<ItemReadyEventArgs>? ItemReady;
    event EventHandler<TestMessageEventArgs>? TestMessageReceived;
    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}

public sealed class OrderBillingRequestedEventArgs : EventArgs
{
    public int OrderId { get; }
    public int? TableNumber { get; }
    public decimal TotalAmount { get; }
    public string CustomerName { get; }

    public OrderBillingRequestedEventArgs(int orderId, int? tableNumber, decimal totalAmount, string customerName)
    {
        OrderId = orderId;
        TableNumber = tableNumber;
        TotalAmount = totalAmount;
        CustomerName = customerName;
    }
}

public sealed record KitchenOrderItemAddedDto(
    int Id,
    string Name,
    int Quantity,
    string? Comment,
    OrderItemStatus Status
);

public sealed class KitchenOrderItemsAddedEventArgs : EventArgs
{
    public int OrderId { get; }
    public int? TableNumber { get; }
    public IReadOnlyList<KitchenOrderItemAddedDto> Items { get; }

    public KitchenOrderItemsAddedEventArgs(int orderId, int? tableNumber, IReadOnlyList<KitchenOrderItemAddedDto> items)
    {
        OrderId = orderId;
        TableNumber = tableNumber;
        Items = items;
    }
}

public sealed class KitchenTicketStatusUpdatedEventArgs : EventArgs
{
    public int TicketId { get; }
    public int OrderId { get; }
    public int? TableNumber { get; }
    public string Name { get; }
    public int Quantity { get; }
    public string? Comment { get; }
    public int OrderItemId { get; }
    public OrderItemStatus Status { get; }

    public KitchenTicketStatusUpdatedEventArgs(
        int ticketId,
        int orderId,
        int? tableNumber,
        string name,
        int quantity,
        string? comment,
        OrderItemStatus status)
    {
        TicketId = ticketId;
        OrderId = orderId;
        TableNumber = tableNumber;
        Name = name;
        Quantity = quantity;
        Comment = comment;
        OrderItemId = ticketId;
        Status = status;
    }
}

public sealed class ItemReadyEventArgs : EventArgs
{
    public int OrderId { get; }
    public int? TableNumber { get; }
    public string ItemName { get; }
    public int Quantity { get; }

    public ItemReadyEventArgs(int orderId, int? tableNumber, string itemName, int quantity)
    {
        OrderId = orderId;
        TableNumber = tableNumber;
        ItemName = itemName;
        Quantity = quantity;
    }
}

public sealed class TestMessageEventArgs : EventArgs
{
    public string Content { get; }

    public TestMessageEventArgs(string content)
    {
        Content = content;
    }
}
