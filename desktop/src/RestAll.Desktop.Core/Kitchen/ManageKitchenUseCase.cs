using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Kitchen;

public interface IManageKitchenUseCase
{
    Task<List<KitchenTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken);
    Task<bool> UpdateTicketStatusAsync(int orderItemId, OrderItemStatus status, CancellationToken cancellationToken);
}

public sealed class ManageKitchenUseCase : IManageKitchenUseCase
{
    private readonly IKitchenGateway _gateway;

    public ManageKitchenUseCase(IKitchenGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<List<KitchenTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken)
    {
        return await _gateway.GetActiveTicketsAsync(cancellationToken);
    }

    public async Task<bool> UpdateTicketStatusAsync(int orderItemId, OrderItemStatus status, CancellationToken cancellationToken)
    {
        return await _gateway.UpdateTicketStatusAsync(orderItemId, status, cancellationToken);
    }
}
