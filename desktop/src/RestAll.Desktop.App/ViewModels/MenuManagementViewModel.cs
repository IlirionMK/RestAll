using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Menu;

namespace RestAll.Desktop.App.ViewModels;

public class MenuManagementViewModel : CancelableViewModelBase
{
    private readonly IManageMenuUseCase _manageMenuUseCase;
    private readonly IAuthenticateUserUseCase _authUseCase;
    
    private ObservableCollection<MenuCategory> _categories = new();
    private MenuItem? _selectedItem;
    private bool _isEditing;
    private string _itemName = string.Empty;
    private string _itemDescription = string.Empty;
    private decimal _itemPrice;
    private string? _itemPhotoUrl;
    private int _selectedCategoryId;
    private ObservableCollection<MenuCategory> _categoryIds = new();
    private Brush _statusColor = Brushes.Black;

    public MenuManagementViewModel(IManageMenuUseCase manageMenuUseCase, IAuthenticateUserUseCase authUseCase)
    {
        _manageMenuUseCase = manageMenuUseCase;
        _authUseCase = authUseCase;
        
        LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        RefreshCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        AddItemCommand = new RelayCommand(StartAddItem);
        SaveItemCommand = new AsyncRelayCommand(SaveItemAsync, CanSaveItem);
        CancelEditCommand = new RelayCommand(CancelEdit);
        DeleteItemCommand = new AsyncRelayCommand(DeleteItemAsync, () => SelectedItem != null);
        ToggleAvailabilityCommand = new AsyncRelayCommand(ToggleAvailabilityAsync, () => SelectedItem != null);
        SelectItemCommand = new RelayCommand<MenuItem>(SelectItem);
        DeleteItemDirectCommand = new AsyncRelayCommand<MenuItem>(DeleteItemDirectAsync);
        ToggleItemAvailabilityCommand = new AsyncRelayCommand<MenuItem>(ToggleItemAvailabilityDirectAsync);
    }

    public ObservableCollection<MenuCategory> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public MenuItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                DeleteItemCommand.NotifyCanExecuteChanged();
                ToggleAvailabilityCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public string ItemName
    {
        get => _itemName;
        set
        {
            if (SetProperty(ref _itemName, value))
            {
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ItemDescription
    {
        get => _itemDescription;
        set
        {
            if (SetProperty(ref _itemDescription, value))
            {
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public decimal ItemPrice
    {
        get => _itemPrice;
        set
        {
            if (SetProperty(ref _itemPrice, value))
            {
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string? ItemPhotoUrl
    {
        get => _itemPhotoUrl;
        set => SetProperty(ref _itemPhotoUrl, value);
    }

    public int SelectedCategoryId
    {
        get => _selectedCategoryId;
        set => SetProperty(ref _selectedCategoryId, value);
    }

    public ObservableCollection<MenuCategory> CategoryIds
    {
        get => _categoryIds;
        set => SetProperty(ref _categoryIds, value);
    }

    public Brush StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    public IAsyncRelayCommand LoadCategoriesCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public ICommand AddItemCommand { get; }
    public IAsyncRelayCommand SaveItemCommand { get; }
    public ICommand CancelEditCommand { get; }
    public IAsyncRelayCommand DeleteItemCommand { get; }
    public IAsyncRelayCommand ToggleAvailabilityCommand { get; }
    public ICommand SelectItemCommand { get; }
    public IAsyncRelayCommand<MenuItem> DeleteItemDirectCommand { get; }
    public IAsyncRelayCommand<MenuItem> ToggleItemAvailabilityCommand { get; }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
    }

    private bool CanSaveItem()
    {
        return !IsLoading && 
               !string.IsNullOrWhiteSpace(ItemName) && 
               ItemPrice > 0 && 
               SelectedCategoryId > 0;
    }

    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading menu categories...";
        StatusColor = Brushes.Blue;

        try
        {
            var categories = await _manageMenuUseCase.GetCategoriesAsync(GetCancellationToken().Token);
            Categories.Clear();
            CategoryIds.Clear();

            foreach (var category in categories)
            {
                Categories.Add(category);
                CategoryIds.Add(category);
            }

            if (Categories.Any())
            {
                SelectedCategoryId = Categories.First().Id;
            }

            StatusMessage = $"Loaded {Categories.Count} categories with {Categories.Sum(c => c.Items.Count)} items";
            StatusColor = Brushes.Green;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading menu: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void StartAddItem()
    {
        IsEditing = true;
        ItemName = string.Empty;
        ItemDescription = string.Empty;
        ItemPrice = 0;
        ItemPhotoUrl = string.Empty;
        SelectedCategoryId = CategoryIds.Any() ? CategoryIds.First().Id : 0;
        SelectedItem = null;
        StatusMessage = "Add new menu item";
        StatusColor = Brushes.Black;
    }

    private async Task SaveItemAsync()
    {
        if (!CanSaveItem())
        {
            StatusMessage = "Please fill in all required fields (Name, Price, Category)";
            StatusColor = Brushes.Red;
            return;
        }

        // Check if user has required role for menu management
        if (!IsAdminOrManager())
        {
            StatusMessage = "Access denied. Admin or Manager role required for menu management.";
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        StatusMessage = SelectedItem != null ? "Updating item..." : "Creating item...";
        StatusColor = Brushes.Blue;

        try
        {
            var request = new MenuItemRequest
            {
                Name = ItemName.Trim(),
                Description = ItemDescription.Trim(),
                Price = ItemPrice,
                PhotoUrl = string.IsNullOrWhiteSpace(ItemPhotoUrl) ? null : ItemPhotoUrl.Trim(),
                MenuCategoryId = SelectedCategoryId
            };

            MenuItem? result;
            if (SelectedItem != null)
            {
                result = await _manageMenuUseCase.UpdateItemAsync(SelectedItem.Id, request, GetCancellationToken().Token);
            }
            else
            {
                result = await _manageMenuUseCase.CreateItemAsync(request, GetCancellationToken().Token);
            }

            if (result != null)
            {
                StatusMessage = SelectedItem != null 
                    ? $"Item '{result.Name}' updated successfully!" 
                    : $"Item '{result.Name}' created successfully!";
                StatusColor = Brushes.Green;
                
                _manageMenuUseCase.InvalidateCache();
                await LoadCategoriesAsync();
                CancelEdit();
            }
            else
            {
                StatusMessage = $"Failed to save item: '{ItemName}'. Backend returned empty response. Check network connection and try again.";
                StatusColor = Brushes.Red;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        SelectedItem = null;
        ItemName = string.Empty;
        ItemDescription = string.Empty;
        ItemPrice = 0;
        ItemPhotoUrl = string.Empty;
        StatusMessage = "";
        StatusColor = Brushes.Black;
    }

    private async Task DeleteItemAsync()
    {
        if (SelectedItem == null)
        {
            return;
        }

        // Check if user has required role for menu management
        if (!IsAdminOrManager())
        {
            StatusMessage = "Access denied. Admin or Manager role required for menu management.";
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        StatusMessage = $"Deleting '{SelectedItem.Name}'...";
        StatusColor = Brushes.Blue;

        try
        {
            var success = await _manageMenuUseCase.DeleteItemAsync(SelectedItem.Id, GetCancellationToken().Token);

            if (success)
            {
                StatusMessage = $"Item '{SelectedItem.Name}' deleted successfully!";
                StatusColor = Brushes.Green;
                
                _manageMenuUseCase.InvalidateCache();
                await LoadCategoriesAsync();
            }
            else
            {
                StatusMessage = "Failed to delete item. Please try again.";
                StatusColor = Brushes.Red;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleAvailabilityAsync()
    {
        if (SelectedItem == null)
        {
            return;
        }

        // Check if user has required role for menu management
        if (!IsAdminOrManager())
        {
            StatusMessage = "Access denied. Admin or Manager role required for menu management.";
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        var newAvailability = !SelectedItem.IsAvailable;
        StatusMessage = $"Setting '{SelectedItem.Name}' as {(newAvailability ? "available" : "unavailable")}...";
        StatusColor = Brushes.Blue;

        try
        {
            var success = await _manageMenuUseCase.ToggleAvailabilityAsync(SelectedItem.Id, newAvailability, GetCancellationToken().Token);

            if (success)
            {
                StatusMessage = $"Item '{SelectedItem.Name}' is now {(newAvailability ? "available" : "unavailable")}!";
                StatusColor = Brushes.Green;
                
                _manageMenuUseCase.InvalidateCache();
                await LoadCategoriesAsync();
            }
            else
            {
                StatusMessage = "Failed to toggle availability. Please try again.";
                StatusColor = Brushes.Red;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool IsAdminOrManager()
    {
        var session = _authUseCase.CurrentSession;
        if (session is null)
        {
            return false;
        }

        var role = session.Role?.ToLowerInvariant();
        return role == "admin" || role == "manager";
    }

    private void SelectItem(MenuItem item)
    {
        if (item == null) return;
        
        SelectedItem = item;
        ItemName = item.Name;
        ItemDescription = item.Description;
        ItemPrice = item.Price;
        ItemPhotoUrl = item.PhotoUrl;
        SelectedCategoryId = item.MenuCategoryId;
        IsEditing = false;
        StatusMessage = $"Selected: {item.Name}";
        StatusColor = Brushes.Black;
    }

    private async Task DeleteItemDirectAsync(MenuItem item)
    {
        if (item == null) return;

        // Check if user has required role for menu management
        if (!IsAdminOrManager())
        {
            StatusMessage = "Access denied. Admin or Manager role required for menu management.";
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        StatusMessage = $"Deleting '{item.Name}'...";
        StatusColor = Brushes.Blue;

        try
        {
            var success = await _manageMenuUseCase.DeleteItemAsync(item.Id, GetCancellationToken().Token);

            if (success)
            {
                StatusMessage = $"Item '{item.Name}' deleted successfully!";
                StatusColor = Brushes.Green;
                
                _manageMenuUseCase.InvalidateCache();
                await LoadCategoriesAsync();
            }
            else
            {
                StatusMessage = "Failed to delete item. Please try again.";
                StatusColor = Brushes.Red;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleItemAvailabilityDirectAsync(MenuItem item)
    {
        if (item == null) return;

        // Check if user has required role for menu management
        if (!IsAdminOrManager())
        {
            StatusMessage = "Access denied. Admin or Manager role required for menu management.";
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        var newAvailability = !item.IsAvailable;
        StatusMessage = $"Setting '{item.Name}' as {(newAvailability ? "available" : "unavailable")}...";
        StatusColor = Brushes.Blue;

        try
        {
            var success = await _manageMenuUseCase.ToggleAvailabilityAsync(item.Id, newAvailability, GetCancellationToken().Token);

            if (success)
            {
                StatusMessage = $"Item '{item.Name}' is now {(newAvailability ? "available" : "unavailable")}!";
                StatusColor = Brushes.Green;
                
                _manageMenuUseCase.InvalidateCache();
                await LoadCategoriesAsync();
            }
            else
            {
                StatusMessage = "Failed to toggle availability. Please try again.";
                StatusColor = Brushes.Red;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
