namespace RestAll.Desktop.Core.Menu;

public interface IGetMenuUseCase
{
    Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken);
}

public sealed class GetMenuUseCase : IGetMenuUseCase
{
    private readonly IMenuGateway _gateway;

    public GetMenuUseCase(IMenuGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _gateway.GetCategoriesAsync(cancellationToken);
    }

    public async Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken)
    {
        return await _gateway.GetItemsAsync(cancellationToken);
    }
}
