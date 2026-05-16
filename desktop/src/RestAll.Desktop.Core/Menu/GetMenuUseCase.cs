using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Core.Offline;

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
    private readonly IOfflineStorage _offline;
    private readonly ILogger<GetMenuUseCase> _logger;

    public GetMenuUseCase(IMenuGateway gateway, ICacheService cache, IOfflineStorage offline, ILogger<GetMenuUseCase> logger)
    {
        _gateway = gateway;
        _cache = cache;
        _offline = offline;
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

            // Save to offline storage and update sync time
            try
            {
                await _offline.SaveMenuCategoriesAsync(categories, cancellationToken);
                await _offline.SetSyncTimeAsync("menu_categories", DateTime.UtcNow, cancellationToken);
                _logger.LogDebug("Saved menu categories to offline storage with sync time");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to save menu categories to offline storage");
            }

            await _cache.SetAsync(cacheKey, categories, TimeSpan.FromMinutes(15), cancellationToken);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching menu categories - trying offline storage");
            try
            {
                if (await _offline.HasDataAsync(cancellationToken))
                {
                    return await _offline.GetMenuCategoriesAsync(cancellationToken);
                }
            }
            catch (Exception iex)
            {
                _logger.LogDebug(iex, "Error reading from offline storage");
            }

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

            // Save to offline storage and update sync time
            try
            {
                await _offline.SaveMenuItemsAsync(items, cancellationToken);
                await _offline.SetSyncTimeAsync("menu_items", DateTime.UtcNow, cancellationToken);
                _logger.LogDebug("Saved menu items to offline storage with sync time");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to save menu items to offline storage");
            }

            await _cache.SetAsync(cacheKey, items, TimeSpan.FromMinutes(15), cancellationToken);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching menu items - trying offline storage");
            try
            {
                if (await _offline.HasDataAsync(cancellationToken))
                {
                    return await _offline.GetMenuItemsAsync(cancellationToken);
                }
            }
            catch (Exception iex)
            {
                _logger.LogDebug(iex, "Error reading menu items from offline storage");
            }

            return new List<MenuItem>();
        }
    }
}
