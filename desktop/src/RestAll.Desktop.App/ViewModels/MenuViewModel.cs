using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Offline;

namespace RestAll.Desktop.App.ViewModels;

public class MenuViewModel : CancelableViewModelBase
{
    private readonly IGetMenuUseCase _menuUseCase;
    private readonly IOfflineStorage _offlineStorage;
    
    private List<MenuCategory> _categories = new();
    private List<MenuItem> _items = new();
    private bool _isOfflineMode;
    private string _lastSyncTime = "";

    public MenuViewModel(IGetMenuUseCase menuUseCase, IOfflineStorage offlineStorage)
    {
        _menuUseCase = menuUseCase;
        _offlineStorage = offlineStorage;
        
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

    public bool IsOfflineMode
    {
        get => _isOfflineMode;
        set => SetProperty(ref _isOfflineMode, value);
    }

    public string LastSyncTime
    {
        get => _lastSyncTime;
        set => SetProperty(ref _lastSyncTime, value);
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
            
            // Check if we have offline data and get sync time
            var hasOfflineData = await _offlineStorage.HasDataAsync(cts.Token);
            var lastSync = await _offlineStorage.GetSyncTimeAsync("menu_categories", cts.Token);
            
            if (lastSync.HasValue)
            {
                LastSyncTime = $"Last updated: {lastSync.Value.ToLocalTime():HH:mm}";
                IsOfflineMode = false;
                StatusMessage = $"Loaded {categories.Count} categories and {items.Count} items. {LastSyncTime}";
            }
            else
            {
                LastSyncTime = "";
                IsOfflineMode = false;
                StatusMessage = $"Loaded {categories.Count} categories and {items.Count} items.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading menu: {ex.Message}. Using offline data if available.";
            IsOfflineMode = true;
            
            // Try to load from offline storage
            try
            {
                var offlineCategories = await _offlineStorage.GetMenuCategoriesAsync(cts.Token);
                var offlineItems = await _offlineStorage.GetMenuItemsAsync(cts.Token);
                
                if (offlineCategories.Any() || offlineItems.Any())
                {
                    Categories = offlineCategories;
                    Items = offlineItems;
                    
                    var lastSync = await _offlineStorage.GetSyncTimeAsync("menu_categories", cts.Token);
                    if (lastSync.HasValue)
                    {
                        LastSyncTime = $"⚠️ Offline mode - Last synced: {lastSync.Value.ToLocalTime():HH:mm}";
                    }
                    else
                    {
                        LastSyncTime = "⚠️ Offline mode - using cached data";
                    }
                }
            }
            catch (Exception)
            {
                StatusMessage = $"Error loading menu: {ex.Message}. No offline data available.";
                LastSyncTime = "❌ No data available";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
