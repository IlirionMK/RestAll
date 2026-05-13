using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Kitchen;

public sealed record KitchenTicket(
    int Id,
    int OrderId,
    int MenuItemId,
    string MenuItemName,
    decimal Price,
    int Quantity,
    string? Comment,
    OrderItemStatus Status,
    string? TableName = null
);
