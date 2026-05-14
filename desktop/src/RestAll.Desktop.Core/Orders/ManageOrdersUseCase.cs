using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Cache;

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
    private readonly ICacheService _cache;
    private readonly ILogger<ManageOrdersUseCase> _logger;

    public ManageOrdersUseCase(IOrderGateway gateway, ICacheService cache, ILogger<ManageOrdersUseCase> logger)
    {
        _gateway = gateway;
        _cache = cache;
        _logger = logger;
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
        var order = await _gateway.CreateOrderAsync(tableId, cancellationToken);
        
        if (order is not null)
        {
            // Invalidate orders cache when new order is created
            await _cache.RemoveAsync("orders", cancellationToken);
        }
        
        return order;
    }

    public async Task<Order?> AddOrderItemsAsync(int orderId, List<OrderItem> items, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding {ItemCount} items to order {OrderId}", items.Count, orderId);
        var order = await _gateway.AddOrderItemsAsync(orderId, items, cancellationToken);
        
        if (order is not null)
        {
            // Update cached order
            var cacheKey = $"order_{orderId}";
            await _cache.SetAsync(cacheKey, order, TimeSpan.FromMinutes(5), cancellationToken);
        }
        
        return order;
    }

    public async Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing item {OrderItemId} from order {OrderId}", orderItemId, orderId);
        var result = await _gateway.RemoveOrderItemAsync(orderId, orderItemId, cancellationToken);
        
        if (result)
        {
            // Invalidate order cache when items are removed
            var cacheKey = $"order_{orderId}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> PayOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Paying order {OrderId}", orderId);
        var result = await _gateway.PayOrderAsync(orderId, cancellationToken);
        
        if (result)
        {
            // Invalidate order cache when order is paid
            var cacheKey = $"order_{orderId}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
        }
        
        return result;
    }
}
