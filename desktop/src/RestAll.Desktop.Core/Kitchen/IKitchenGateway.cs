using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Kitchen;

public interface IKitchenGateway
{
    Task<List<KitchenTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken);
    Task<bool> UpdateTicketStatusAsync(int orderItemId, OrderItemStatus status, CancellationToken cancellationToken);
}
