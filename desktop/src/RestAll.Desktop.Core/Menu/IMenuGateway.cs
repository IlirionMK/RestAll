namespace RestAll.Desktop.Core.Menu;

public interface IMenuGateway
{
    Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken);
    Task<MenuItem?> CreateItemAsync(MenuItemRequest request, CancellationToken cancellationToken);
    Task<MenuItem?> UpdateItemAsync(int id, MenuItemRequest request, CancellationToken cancellationToken);
    Task<bool> ToggleAvailabilityAsync(int id, bool isAvailable, CancellationToken cancellationToken);
    Task<bool> DeleteItemAsync(int id, CancellationToken cancellationToken);
}

public class MenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? PhotoUrl { get; set; }
    public int MenuCategoryId { get; set; }
}
