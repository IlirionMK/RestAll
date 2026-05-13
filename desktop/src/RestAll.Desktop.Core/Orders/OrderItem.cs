namespace RestAll.Desktop.Core.Orders;

public sealed record OrderItem(
    int Id,
    int OrderId,
    int MenuItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? Comment,
    OrderItemStatus Status
);
