using Microsoft.Extensions.Logging;

namespace RestAll.Desktop.Core.Menu;

public interface IManageMenuUseCase
{
    Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<MenuItem?> CreateItemAsync(MenuItemRequest request, CancellationToken cancellationToken);
    Task<MenuItem?> UpdateItemAsync(int id, MenuItemRequest request, CancellationToken cancellationToken);
    Task<bool> ToggleAvailabilityAsync(int id, bool isAvailable, CancellationToken cancellationToken);
    Task<bool> DeleteItemAsync(int id, CancellationToken cancellationToken);
    void InvalidateCache();
}

public sealed class ManageMenuUseCase : IManageMenuUseCase
{
    private readonly IMenuGateway _gateway;
    private readonly ILogger<ManageMenuUseCase> _logger;

    public ManageMenuUseCase(IMenuGateway gateway, ILogger<ManageMenuUseCase> logger)
    {
        _gateway = gateway;
        _logger = logger;
    }

    public async Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching menu categories for management");
        return await _gateway.GetCategoriesAsync(cancellationToken);
    }

    public async Task<MenuItem?> CreateItemAsync(MenuItemRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating menu item: {Name} in category {CategoryId}", request.Name, request.MenuCategoryId);
        
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Menu item creation failed: name is required");
            return null;
        }

        if (request.Price <= 0)
        {
            _logger.LogWarning("Menu item creation failed: price must be positive");
            return null;
        }

        return await _gateway.CreateItemAsync(request, cancellationToken);
    }

    public async Task<MenuItem?> UpdateItemAsync(int id, MenuItemRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating menu item {Id}: {Name}", id, request.Name);
        
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Menu item update failed: name is required");
            return null;
        }

        if (request.Price <= 0)
        {
            _logger.LogWarning("Menu item update failed: price must be positive");
            return null;
        }

        return await _gateway.UpdateItemAsync(id, request, cancellationToken);
    }

    public async Task<bool> ToggleAvailabilityAsync(int id, bool isAvailable, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Toggling availability for menu item {Id}: {IsAvailable}", id, isAvailable);
        return await _gateway.ToggleAvailabilityAsync(id, isAvailable, cancellationToken);
    }

    public async Task<bool> DeleteItemAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting menu item {Id}", id);
        return await _gateway.DeleteItemAsync(id, cancellationToken);
    }

    public void InvalidateCache()
    {
        _logger.LogInformation("Menu cache invalidated");
    }
}
