namespace RestAll.Desktop.Core.Menu;

public interface IMenuGateway
{
    Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken);
}
