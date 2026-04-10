using RestAll.Desktop.Core.Menu;

namespace RestAll.Desktop.App.ViewModels;

public class MenuViewModel : CancelableViewModelBase
{
    private readonly IGetMenuUseCase _menuUseCase;
    
    private List<MenuCategory> _categories = new();
    private List<MenuItem> _items = new();

    public MenuViewModel(IGetMenuUseCase menuUseCase)
    {
        _menuUseCase = menuUseCase;
        
        LoadMenuCommand = new AsyncRelayCommand(LoadMenuAsync, () => !IsLoading);
    }

    public List<MenuCategory> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public List<MenuItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public IAsyncRelayCommand LoadMenuCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadMenuCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadMenuAsync()
    {
        IsLoading = true;
        StatusMessage = "";
        var cts = GetCancellationToken();

        try
        {
            var categories = await _menuUseCase.GetCategoriesAsync(cts.Token);
            var items = await _menuUseCase.GetItemsAsync(cts.Token);

            Categories = categories;
            Items = items;
            StatusMessage = $"Loaded {categories.Count} categories and {items.Count} items.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading menu: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
