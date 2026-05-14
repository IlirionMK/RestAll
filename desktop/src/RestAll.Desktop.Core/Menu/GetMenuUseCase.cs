using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Cache;

namespace RestAll.Desktop.Core.Menu;

public interface IGetMenuUseCase
{
    Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken);
}

public sealed class GetMenuUseCase : IGetMenuUseCase
{
    private readonly IMenuGateway _gateway;
    private readonly ICacheService _cache;
    private readonly ILogger<GetMenuUseCase> _logger;

    public GetMenuUseCase(IMenuGateway gateway, ICacheService cache, ILogger<GetMenuUseCase> logger)
    {
        _gateway = gateway;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "menu_categories";
        
        try
        {
            var cached = await _cache.GetAsync<List<MenuCategory>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache HIT for {CacheKey}", cacheKey);
                return cached;
            }

            _logger.LogInformation("Cache MISS for {CacheKey} - fetching from API", cacheKey);
            var categories = await _gateway.GetCategoriesAsync(cancellationToken);
            
            await _cache.SetAsync(cacheKey, categories, TimeSpan.FromMinutes(15), cancellationToken);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching menu categories");
            return new List<MenuCategory>();
        }
    }

    public async Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "menu_items";
        
        try
        {
            var cached = await _cache.GetAsync<List<MenuItem>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache HIT for {CacheKey}", cacheKey);
                return cached;
            }

            _logger.LogInformation("Cache MISS for {CacheKey} - fetching from API", cacheKey);
            var items = await _gateway.GetItemsAsync(cancellationToken);
            
            await _cache.SetAsync(cacheKey, items, TimeSpan.FromMinutes(15), cancellationToken);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching menu items");
            return new List<MenuItem>();
        }
    }
}
