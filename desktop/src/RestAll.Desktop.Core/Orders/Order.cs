namespace RestAll.Desktop.Core.Orders;

public sealed record Order(
    int Id,
    int TableId,
    int UserId,
    decimal TotalAmount,
    OrderStatus Status,
    List<OrderItem> Items
);
