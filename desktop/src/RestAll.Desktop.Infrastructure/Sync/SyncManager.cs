using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Infrastructure.Sync;

public class SyncManager : ISyncManager
{
    private readonly IOfflineStorage _offlineStorage;
    private readonly IOrderGateway _orderGateway;
    private readonly ILogger<SyncManager> _logger;
    private CancellationTokenSource? _cts;
    private Task? _runTask;

    public SyncManager(IOfflineStorage offlineStorage, IOrderGateway orderGateway, ILogger<SyncManager> logger)
    {
        _offlineStorage = offlineStorage;
        _orderGateway = orderGateway;
        _logger = logger;
    }

    public void Start()
    {
        if (_runTask is not null && !_runTask.IsCompleted) return;
        _cts = new CancellationTokenSource();
        _runTask = Task.Run(() => RunAsync(_cts.Token));
        _logger.LogInformation("SyncManager started - will sync pending orders and payments");
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
        }
        catch { }
    }

    // Legacy methods - kept for backward compatibility but now use new pending tables
    public async Task<Order> EnqueueCreateOrderAsync(int tableId, CancellationToken cancellationToken = default)
    {
        var localId = await _offlineStorage.CreatePendingOrderAsync(tableId, null, cancellationToken);
        
        // Return temporary order with negative ID
        var tempOrder = new Order(-localId, tableId, 0, 0m, OrderStatus.Pending, new List<OrderItem>());
        return tempOrder;
    }

    public async Task EnqueueAddItemsAsync(int localOrderId, List<OrderItem> items, CancellationToken cancellationToken = default)
    {
        var actualLocalId = localOrderId < 0 ? -localOrderId : localOrderId;
        
        foreach (var item in items)
        {
            var pendingItem = new PendingOrderItem(
                item.MenuItemId,
                item.Name ?? "Unknown",
                item.Price,
                item.Quantity,
                item.Comment,
                "add"
            );
            await _offlineStorage.AddPendingOrderItemAsync(actualLocalId, pendingItem, cancellationToken);
        }
        
        _logger.LogInformation("Enqueued {Count} items for local order {LocalOrderId}", items.Count, actualLocalId);
    }

    public async Task EnqueueRemoveItemAsync(int localOrderId, int orderItemId, CancellationToken cancellationToken = default)
    {
        var actualLocalId = localOrderId < 0 ? -localOrderId : localOrderId;
        await _offlineStorage.RemovePendingOrderItemAsync(actualLocalId, orderItemId, cancellationToken);
        _logger.LogInformation("Enqueued RemoveItem for local order {LocalOrderId}, item {OrderItemId}", actualLocalId, orderItemId);
    }

    public async Task EnqueuePayOrderAsync(int serverOrderId, CancellationToken cancellationToken = default)
    {
        // We need to get the order amount first - this will be handled when PayOrder fails
        // This method is kept for backward compatibility
        _logger.LogDebug("EnqueuePayOrder called for order {ServerOrderId} - will be handled by ManageOrdersUseCase", serverOrderId);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Sync pending orders
                await SyncPendingOrdersAsync(cancellationToken);
                
                // Sync pending payments
                await SyncPendingPaymentsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncManager loop error");
            }

            // Sleep before next attempt
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (TaskCanceledException) { }
        }
    }

    private async Task SyncPendingOrdersAsync(CancellationToken cancellationToken)
    {
        var pendingOrders = await _offlineStorage.GetPendingOrdersAsync(cancellationToken);
        
        foreach (var pendingOrder in pendingOrders)
        {
            if (cancellationToken.IsCancellationRequested) break;
            
            // Skip orders already being synced
            if (pendingOrder.Status == "syncing") continue;
            
            try
            {
                _logger.LogInformation("Syncing pending order {LocalId} for table {TableId}", pendingOrder.LocalId, pendingOrder.TableId);
                
                // Mark as syncing
                await _offlineStorage.MarkOrderSyncingAsync(pendingOrder.LocalId, cancellationToken);
                
                // Create order on server
                var createdOrder = await _orderGateway.CreateOrderAsync(pendingOrder.TableId, cancellationToken);
                
                if (createdOrder is not null)
                {
                    _logger.LogInformation("Successfully created server order {ServerId} for local order {LocalId}", createdOrder.Id, pendingOrder.LocalId);
                    
                    // Mark order as synced
                    await _offlineStorage.MarkOrderSyncedAsync(pendingOrder.LocalId, createdOrder.Id, cancellationToken);
                    
                    // Now sync the items
                    await SyncOrderItemsAsync(pendingOrder.LocalId, createdOrder.Id, cancellationToken);
                    
                    // Save the order to local cache
                    await _offlineStorage.SaveOrderAsync(createdOrder, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Failed to create order for local order {LocalId} - server returned null", pendingOrder.LocalId);
                    await _offlineStorage.MarkOrderFailedAsync(pendingOrder.LocalId, "Server returned null", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing pending order {LocalId}", pendingOrder.LocalId);
                await _offlineStorage.MarkOrderFailedAsync(pendingOrder.LocalId, ex.Message, cancellationToken);
            }
        }
    }

    private async Task SyncOrderItemsAsync(int localOrderId, int serverOrderId, CancellationToken cancellationToken)
    {
        var pendingItems = await _offlineStorage.GetPendingOrderItemsAsync(localOrderId, cancellationToken);
        
        foreach (var item in pendingItems)
        {
            if (cancellationToken.IsCancellationRequested) break;
            
            try
            {
                if (item.Operation == "add")
                {
                    _logger.LogDebug("Adding item {MenuItemId} to server order {ServerOrderId}", item.MenuItemId, serverOrderId);
                    
                    var orderItem = new OrderItem(
                        0, // Will be assigned by server
                        serverOrderId,
                        item.MenuItemId,
                        item.Name,
                        item.Price,
                        item.Quantity,
                        item.Comment,
                        OrderItemStatus.Pending
                    );
                    
                    await _orderGateway.AddOrderItemsAsync(serverOrderId, new List<OrderItem> { orderItem }, cancellationToken);
                    
                    // TODO: Get the actual order item ID from response and mark as synced
                    // For now, just mark all items as synced
                    await _offlineStorage.MarkPendingOrderItemSyncedAsync(item.MenuItemId, cancellationToken);
                }
                else if (item.Operation == "remove")
                {
                    _logger.LogDebug("Removing item {MenuItemId} from server order {ServerOrderId}", item.MenuItemId, serverOrderId);
                    // TODO: Implement remove logic - need to find the actual order item ID
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing item {MenuItemId} for order {ServerOrderId}", item.MenuItemId, serverOrderId);
            }
        }
    }

    private async Task SyncPendingPaymentsAsync(CancellationToken cancellationToken)
    {
        var pendingPayments = await _offlineStorage.GetPendingPaymentsAsync(cancellationToken);
        
        foreach (var payment in pendingPayments)
        {
            if (cancellationToken.IsCancellationRequested) break;
            
            // Skip payments already being synced
            if (payment.Status == "syncing") continue;
            
            try
            {
                _logger.LogInformation("Syncing pending payment {PaymentId} for order {OrderId}", payment.Id, payment.OrderServerId);
                
                // Process payment on server
                var result = await _orderGateway.PayOrderAsync(payment.OrderServerId, cancellationToken);
                
                if (result)
                {
                    _logger.LogInformation("Successfully synced payment {PaymentId} for order {OrderId}", payment.Id, payment.OrderServerId);
                    await _offlineStorage.MarkPaymentSyncedAsync(payment.Id, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Payment failed for order {OrderId}", payment.OrderServerId);
                    await _offlineStorage.MarkPaymentFailedAsync(payment.Id, "Payment failed", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing payment {PaymentId} for order {OrderId}", payment.Id, payment.OrderServerId);
                await _offlineStorage.MarkPaymentFailedAsync(payment.Id, ex.Message, cancellationToken);
            }
        }
    }
}

