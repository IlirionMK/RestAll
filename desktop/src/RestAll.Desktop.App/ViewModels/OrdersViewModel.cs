using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Realtime;

namespace RestAll.Desktop.App.ViewModels;

public class OrdersViewModel : CancelableViewModelBase
{
    private readonly IManageOrdersUseCase _ordersUseCase;
    private readonly IRealtimeService _realtimeService;
    
    private List<Order> _orders = new();
    private string _newOrderTableId = "";
    private string _addItemOrderId = "";
    private string _addItemMenuItemId = "";
    private string _addItemQuantity = "1";
    private string _addItemComment = "";
    private string _removeItemOrderId = "";
    private string _removeItemOrderItemId = "";

    public OrdersViewModel(IManageOrdersUseCase ordersUseCase, IRealtimeService realtimeService)
    {
        _ordersUseCase = ordersUseCase;
        _realtimeService = realtimeService;
        _realtimeService.OrderBillingRequested += OnRealtimeRefreshRequested;
        _realtimeService.KitchenOrderItemsAdded += OnRealtimeRefreshRequested;
        _realtimeService.KitchenTicketStatusUpdated += OnRealtimeRefreshRequested;
        _realtimeService.ItemReady += OnRealtimeRefreshRequested;
        
        LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync, () => !IsLoading);
        PayOrderCommand = new AsyncRelayCommand<int>(PayOrderAsync, _ => !IsLoading);
        CreateOrderCommand = new AsyncRelayCommand(CreateOrderAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(NewOrderTableId));
        AddItemCommand = new AsyncRelayCommand(AddItemAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(AddItemOrderId) && !string.IsNullOrWhiteSpace(AddItemMenuItemId));
        RemoveItemCommand = new AsyncRelayCommand(RemoveItemAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(RemoveItemOrderId) && !string.IsNullOrWhiteSpace(RemoveItemOrderItemId));
    }

    public List<Order> Orders
    {
        get => _orders;
        set => SetProperty(ref _orders, value);
    }

    public string NewOrderTableId
    {
        get => _newOrderTableId;
        set => SetProperty(ref _newOrderTableId, value);
    }

    public string AddItemOrderId
    {
        get => _addItemOrderId;
        set => SetProperty(ref _addItemOrderId, value);
    }

    public string AddItemMenuItemId
    {
        get => _addItemMenuItemId;
        set => SetProperty(ref _addItemMenuItemId, value);
    }

    public string AddItemQuantity
    {
        get => _addItemQuantity;
        set => SetProperty(ref _addItemQuantity, value);
    }

    public string AddItemComment
    {
        get => _addItemComment;
        set => SetProperty(ref _addItemComment, value);
    }

    public string RemoveItemOrderId
    {
        get => _removeItemOrderId;
        set => SetProperty(ref _removeItemOrderId, value);
    }

    public string RemoveItemOrderItemId
    {
        get => _removeItemOrderItemId;
        set => SetProperty(ref _removeItemOrderItemId, value);
    }

    public IAsyncRelayCommand LoadOrdersCommand { get; }
    public IAsyncRelayCommand<int> PayOrderCommand { get; }
    public IAsyncRelayCommand CreateOrderCommand { get; }
    public IAsyncRelayCommand AddItemCommand { get; }
    public IAsyncRelayCommand RemoveItemCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadOrdersCommand.NotifyCanExecuteChanged();
        PayOrderCommand.NotifyCanExecuteChanged();
        CreateOrderCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
    }

    protected override void OnPropertyChanged(string? propertyName)
    {
        base.OnPropertyChanged(propertyName);
        CreateOrderCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadOrdersAsync()
    {
        IsLoading = true;
        StatusMessage = "";

        try
        {
            var orders = await _ordersUseCase.GetOrdersAsync(GetCancellationToken().Token);
            Orders = orders;
            StatusMessage = $"Loaded {orders.Count} orders.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading orders: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnRealtimeRefreshRequested(object? sender, EventArgs e)
    {
        if (IsLoading)
        {
            return;
        }

        _ = LoadOrdersAsync();
    }

    private async Task PayOrderAsync(int orderId)
    {
        try
        {
            var result = await _ordersUseCase.PayOrderAsync(orderId, GetCancellationToken().Token);
            
            if (result)
            {
                StatusMessage = $"Order {orderId} paid successfully.";
                await LoadOrdersAsync();
            }
            else
            {
                StatusMessage = $"Failed to pay order {orderId}.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error paying order: {ex.Message}";
        }
    }

    private async Task CreateOrderAsync()
    {
        if (!int.TryParse(NewOrderTableId, out var tableId))
        {
            StatusMessage = "Invalid table ID.";
            return;
        }

        try
        {
            var order = await _ordersUseCase.CreateOrderAsync(tableId, GetCancellationToken().Token);
            
            if (order is not null)
            {
                StatusMessage = $"Order {order.Id} created successfully.";
                NewOrderTableId = "";
                await LoadOrdersAsync();
            }
            else
            {
                StatusMessage = "Failed to create order.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating order: {ex.Message}";
        }
    }

    private async Task AddItemAsync()
    {
        if (!int.TryParse(AddItemOrderId, out var orderId) ||
            !int.TryParse(AddItemMenuItemId, out var menuItemId) ||
            !int.TryParse(AddItemQuantity, out var quantity))
        {
            StatusMessage = "Invalid input values.";
            return;
        }

        try
        {
            var items = new List<OrderItem>
            {
                new OrderItem(0, orderId, menuItemId, "", 0, quantity, AddItemComment, OrderItemStatus.Pending)
            };

            var order = await _ordersUseCase.AddOrderItemsAsync(orderId, items, GetCancellationToken().Token);
            
            if (order is not null)
            {
                StatusMessage = $"Item added to order {orderId} successfully.";
                AddItemOrderId = "";
                AddItemMenuItemId = "";
                AddItemQuantity = "1";
                AddItemComment = "";
                await LoadOrdersAsync();
            }
            else
            {
                StatusMessage = "Failed to add item to order.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding item: {ex.Message}";
        }
    }

    private async Task RemoveItemAsync()
    {
        if (!int.TryParse(RemoveItemOrderId, out var orderId) ||
            !int.TryParse(RemoveItemOrderItemId, out var orderItemId))
        {
            StatusMessage = "Invalid input values.";
            return;
        }

        try
        {
            var result = await _ordersUseCase.RemoveOrderItemAsync(orderId, orderItemId, GetCancellationToken().Token);
            
            if (result)
            {
                StatusMessage = $"Item removed from order {orderId} successfully.";
                RemoveItemOrderId = "";
                RemoveItemOrderItemId = "";
                await LoadOrdersAsync();
            }
            else
            {
                StatusMessage = "Failed to remove item from order.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error removing item: {ex.Message}";
        }
    }

    protected override void OnDispose()
    {
        _realtimeService.OrderBillingRequested -= OnRealtimeRefreshRequested;
        _realtimeService.KitchenOrderItemsAdded -= OnRealtimeRefreshRequested;
        _realtimeService.KitchenTicketStatusUpdated -= OnRealtimeRefreshRequested;
        _realtimeService.ItemReady -= OnRealtimeRefreshRequested;
    }
}
