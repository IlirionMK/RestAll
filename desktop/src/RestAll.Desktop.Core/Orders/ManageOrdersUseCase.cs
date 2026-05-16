using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Core.Offline;

namespace RestAll.Desktop.Core.Orders;

public interface IManageOrdersUseCase
{
    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken);
    Task<Order?> CreateOrderAsync(int tableId, CancellationToken cancellationToken);
    Task<Order?> AddOrderItemsAsync(int orderId, List<OrderItem> items, CancellationToken cancellationToken);
    Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId, CancellationToken cancellationToken);
    Task<bool> PayOrderAsync(int orderId, CancellationToken cancellationToken);
    Task<bool> RequestBillAsync(int orderId, CancellationToken cancellationToken);
}

public sealed class ManageOrdersUseCase : IManageOrdersUseCase
{
    private readonly IOrderGateway _gateway;
    private readonly ICacheService _cache;
    private readonly ILogger<ManageOrdersUseCase> _logger;
    private readonly ISyncManager? _syncManager;
    private readonly IOfflineStorage? _offlineStorage;

    public ManageOrdersUseCase(IOrderGateway gateway, ICacheService cache, ILogger<ManageOrdersUseCase> logger)
    {
        _gateway = gateway;
        _cache = cache;
        _logger = logger;
    }

    // Constructor with sync manager and offline storage
    public ManageOrdersUseCase(IOrderGateway gateway, ICacheService cache, ILogger<ManageOrdersUseCase> logger, ISyncManager syncManager, IOfflineStorage offlineStorage)
        : this(gateway, cache, logger)
    {
        _syncManager = syncManager;
        _offlineStorage = offlineStorage;
    }

    public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "orders";
        
        try
        {
            var cached = await _cache.GetAsync<List<Order>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache HIT for {CacheKey}", cacheKey);
                return cached;
            }

            _logger.LogInformation("Cache MISS for {CacheKey} - fetching from API", cacheKey);
            var orders = await _gateway.GetOrdersAsync(cancellationToken);
            
            await _cache.SetAsync(cacheKey, orders, TimeSpan.FromMinutes(10), cancellationToken);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders");
            return new List<Order>();
        }
    }

    public async Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        var cacheKey = $"order_{id}";
        
        try
        {
            var cached = await _cache.GetAsync<Order>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache HIT for {CacheKey}", cacheKey);
                return cached;
            }

            _logger.LogInformation("Cache MISS for {CacheKey} - fetching from API", cacheKey);
            var order = await _gateway.GetOrderAsync(id, cancellationToken);
            
            if (order is not null)
            {
                await _cache.SetAsync(cacheKey, order, TimeSpan.FromMinutes(5), cancellationToken);
            }
            
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId}", id);
            return null;
        }
    }

    public async Task<Order?> CreateOrderAsync(int tableId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for table {TableId}", tableId);

        try
        {
            var order = await _gateway.CreateOrderAsync(tableId, cancellationToken);
            if (order is not null)
            {
                // Invalidate orders cache when new order is created
                await _cache.RemoveAsync("orders", cancellationToken);
            }

            return order;
        }
        catch (Exception ex) when (_offlineStorage is not null)
        {
            // Network error - create offline draft
            _logger.LogWarning(ex, "CreateOrder failed, creating offline draft for table {TableId}", tableId);
            
            var localId = await _offlineStorage.CreatePendingOrderAsync(tableId, null, cancellationToken);
            
            // Return mock order with negative ID to indicate offline mode
            var offlineOrder = new Order(
                -localId,  // Negative ID = local only
                tableId,
                0,
                0m,
                OrderStatus.Pending,
                new List<OrderItem>()
            );
            
            // Invalidate cache to reflect pending order
            await _cache.RemoveAsync("orders", cancellationToken);
            return offlineOrder;
        }
    }

    public async Task<Order?> AddOrderItemsAsync(int orderId, List<OrderItem> items, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding {ItemCount} items to order {OrderId}", items.Count, orderId);
        
        try
        {
            var order = await _gateway.AddOrderItemsAsync(orderId, items, cancellationToken);
            
            if (order is not null)
            {
                // Update cached order
                var cacheKey = $"order_{orderId}";
                await _cache.SetAsync(cacheKey, order, TimeSpan.FromMinutes(5), cancellationToken);
            }
            
            return order;
        }
        catch (Exception ex) when (orderId < 0 && _offlineStorage is not null)
        {
            // Offline order (negative ID) - queue the operation
            _logger.LogWarning(ex, "AddOrderItems failed for offline order {OrderId}, queuing for sync", orderId);
            
            var localOrderId = -orderId;
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
                await _offlineStorage.AddPendingOrderItemAsync(localOrderId, pendingItem, cancellationToken);
            }
            
            // Return a mock order with items added locally
            return new Order(orderId, 0, 0, 0m, OrderStatus.Pending, items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding items to order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing item {OrderItemId} from order {OrderId}", orderItemId, orderId);
        
        try
        {
            var result = await _gateway.RemoveOrderItemAsync(orderId, orderItemId, cancellationToken);
            
            if (result)
            {
                // Invalidate order cache when items are removed
                var cacheKey = $"order_{orderId}";
                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }
            
            return result;
        }
        catch (Exception ex) when (orderId < 0 && _offlineStorage is not null)
        {
            // Offline order (negative ID) - queue the removal
            _logger.LogWarning(ex, "RemoveOrderItem failed for offline order {OrderId}, queuing for sync", orderId);
            
            var localOrderId = -orderId;
            await _offlineStorage.RemovePendingOrderItemAsync(localOrderId, orderItemId, cancellationToken);
            return true; // Assume success for offline operations
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item {OrderItemId} from order {OrderId}", orderItemId, orderId);
            throw;
        }
    }

    public async Task<bool> PayOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Paying order {OrderId}", orderId);
        
        try
        {
            var result = await _gateway.PayOrderAsync(orderId, cancellationToken);
            
            if (result)
            {
                // Invalidate order cache when order is paid
                var cacheKey = $"order_{orderId}";
                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }
            
            return result;
        }
        catch (Exception ex) when (orderId > 0 && _offlineStorage is not null)
        {
            // Online order but API failed - queue payment for later sync
            _logger.LogWarning(ex, "PayOrder failed for order {OrderId}, queuing for sync", orderId);
            
            // Get order details to get the amount
            var order = await GetOrderAsync(orderId, cancellationToken);
            if (order is not null)
            {
                await _offlineStorage.AddPendingPaymentAsync(orderId, order.TotalAmount, null, cancellationToken);
            }
            
            return true; // Assume will succeed when synced
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error paying order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<bool> RequestBillAsync(int orderId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requesting bill for order {OrderId}", orderId);
        return await _gateway.RequestBillAsync(orderId, cancellationToken);
    }
}
