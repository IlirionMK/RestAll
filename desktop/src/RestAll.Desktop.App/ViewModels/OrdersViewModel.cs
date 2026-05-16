using System.Windows.Media;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.Core.Offline;

namespace RestAll.Desktop.App.ViewModels;

public class OrdersViewModel : CancelableViewModelBase
{
    private readonly IManageOrdersUseCase _ordersUseCase;
    private readonly IRealtimeService _realtimeService;
    private readonly IOfflineStorage? _offlineStorage;
    
    private List<Order> _orders = new();
    private string _newOrderTableId = "";
    private string _addItemOrderId = "";
    private string _addItemMenuItemId = "";
    private string _addItemQuantity = "1";
    private string _addItemComment = "";
    private string _removeItemOrderId = "";
    private string _removeItemOrderItemId = "";
    private int? _billRequestedOrderId;
    private Brush _billStatusColor = Brushes.Black;
    private string _requestBillOrderId = "";
    private int _pendingSyncCount = 0;

    public OrdersViewModel(IManageOrdersUseCase ordersUseCase, IRealtimeService realtimeService, IOfflineStorage? offlineStorage = null)
    {
        _ordersUseCase = ordersUseCase;
        _realtimeService = realtimeService;
        _offlineStorage = offlineStorage;
        _realtimeService.OrderBillingRequested += OnRealtimeRefreshRequested;
        _realtimeService.KitchenOrderItemsAdded += OnRealtimeRefreshRequested;
        _realtimeService.KitchenTicketStatusUpdated += OnRealtimeRefreshRequested;
        _realtimeService.ItemReady += OnRealtimeRefreshRequested;
        
        LoadOrdersCommand = new AsyncRelayCommand(async () => await LoadOrdersAsync(), () => !IsLoading);
        PayOrderCommand = new AsyncRelayCommand<int>(PayOrderAsync, _ => !IsLoading);
        CreateOrderCommand = new AsyncRelayCommand(CreateOrderAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(NewOrderTableId));
        AddItemCommand = new AsyncRelayCommand(AddItemAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(AddItemOrderId) && !string.IsNullOrWhiteSpace(AddItemMenuItemId));
        RemoveItemCommand = new AsyncRelayCommand(RemoveItemAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(RemoveItemOrderId) && !string.IsNullOrWhiteSpace(RemoveItemOrderItemId));
        RequestBillCommand = new AsyncRelayCommand<int>(RequestBillAsync, _ => !IsLoading);
        SyncNowCommand = new AsyncRelayCommand(SyncNowAsync, () => !IsLoading);
        
        // Start timer to refresh pending count every 10 seconds
        StartPendingCountTimer();
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

    public int? BillRequestedOrderId
    {
        get => _billRequestedOrderId;
        set => SetProperty(ref _billRequestedOrderId, value);
    }

    public Brush BillStatusColor
    {
        get => _billStatusColor;
        set => SetProperty(ref _billStatusColor, value);
    }

    public string RequestBillOrderId
    {
        get => _requestBillOrderId;
        set => SetProperty(ref _requestBillOrderId, value);
    }

    public int PendingSyncCount
    {
        get => _pendingSyncCount;
        set => SetProperty(ref _pendingSyncCount, value);
    }

    public IAsyncRelayCommand LoadOrdersCommand { get; }
    public IAsyncRelayCommand<int> PayOrderCommand { get; }
    public IAsyncRelayCommand CreateOrderCommand { get; }
    public IAsyncRelayCommand AddItemCommand { get; }
    public IAsyncRelayCommand RemoveItemCommand { get; }
    public IAsyncRelayCommand<int> RequestBillCommand { get; }
    public IAsyncRelayCommand SyncNowCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadOrdersCommand.NotifyCanExecuteChanged();
        PayOrderCommand.NotifyCanExecuteChanged();
        CreateOrderCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
        RequestBillCommand.NotifyCanExecuteChanged();
        SyncNowCommand.NotifyCanExecuteChanged();
    }

    protected override void OnPropertyChanged(string? propertyName)
    {
        base.OnPropertyChanged(propertyName);
        CreateOrderCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
    }

    // Helper method for UI binding - check if order is offline (negative ID)
    public static bool IsOfflineOrder(Order order)
    {
        return order.Id < 0;
    }

    private async Task LoadOrdersAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        StatusMessage = "";

        try
        {
            // Use provided token or create a new one if none provided
            var token = cancellationToken != default ? cancellationToken : GetCancellationToken().Token;
            var orders = await _ordersUseCase.GetOrdersAsync(token);
            Orders = orders;
            
            // Count pending sync operations (negative IDs = offline orders)
            PendingSyncCount = orders.Count(o => o.Id < 0);
            
            if (PendingSyncCount > 0)
            {
                StatusMessage = $"Loaded {orders.Count} orders ({PendingSyncCount} pending sync).";
            }
            else
            {
                StatusMessage = $"Loaded {orders.Count} orders.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Loading orders was cancelled.";
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
        // Use dedicated CancellationToken to avoid cancellation from profile refreshes
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        try
        {
            var result = await _ordersUseCase.PayOrderAsync(orderId, cts.Token);
            
            if (result)
            {
                StatusMessage = $"Order {orderId} paid successfully.";
                
                // Use separate token for refresh
                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await LoadOrdersAsync(refreshCts.Token);
            }
            else
            {
                StatusMessage = $"Failed to pay order {orderId}.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Payment operation timed out.";
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
            StatusMessage = "Invalid table ID. Please enter a valid number.";
            return;
        }

        // Use dedicated CancellationToken to avoid cancellation from profile refreshes
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        IsLoading = true;
        StatusMessage = $"Creating order for table {tableId}...";

        try
        {
            var order = await _ordersUseCase.CreateOrderAsync(tableId, cts.Token);
            
            if (order is not null)
            {
                StatusMessage = order.Id < 0 
                    ? $"Order created (offline mode - ID: {order.Id}). Will sync when online." 
                    : $"Order {order.Id} created successfully.";
                NewOrderTableId = "";
                
                // Use separate token for refresh
                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await LoadOrdersAsync(refreshCts.Token);
            }
            else
            {
                StatusMessage = $"Failed to create order for table {tableId}. Backend returned empty response. Check network connection and try again.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Order creation timed out.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating order: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
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

        // Use dedicated CancellationToken to avoid cancellation from profile refreshes
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        try
        {
            var items = new List<OrderItem>
            {
                new OrderItem(0, orderId, menuItemId, "", 0, quantity, AddItemComment, OrderItemStatus.Pending)
            };

            var order = await _ordersUseCase.AddOrderItemsAsync(orderId, items, cts.Token);
            
            if (order is not null)
            {
                StatusMessage = $"Item added to order {orderId} successfully.";
                AddItemOrderId = "";
                AddItemMenuItemId = "";
                AddItemQuantity = "1";
                AddItemComment = "";
                
                // Use separate token for refresh
                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await LoadOrdersAsync(refreshCts.Token);
            }
            else
            {
                StatusMessage = "Failed to add item to order.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Add item operation timed out.";
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

        // Use dedicated CancellationToken to avoid cancellation from profile refreshes
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        try
        {
            var result = await _ordersUseCase.RemoveOrderItemAsync(orderId, orderItemId, cts.Token);
            
            if (result)
            {
                StatusMessage = $"Item removed from order {orderId} successfully.";
                RemoveItemOrderId = "";
                RemoveItemOrderItemId = "";
                
                // Use separate token for refresh
                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await LoadOrdersAsync(refreshCts.Token);
            }
            else
            {
                StatusMessage = "Failed to remove item from order.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Remove item operation timed out.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error removing item: {ex.Message}";
        }
    }

    private async Task RequestBillAsync(int orderId)
    {
        // Use dedicated CancellationToken to avoid cancellation from profile refreshes
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        try
        {
            var result = await _ordersUseCase.RequestBillAsync(orderId, cts.Token);
            
            if (result)
            {
                BillRequestedOrderId = orderId;
                BillStatusColor = Brushes.Orange;
                StatusMessage = $"Bill requested for order {orderId}. Waiting for waiter...";
                
                // Use separate token for refresh
                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await LoadOrdersAsync(refreshCts.Token);
            }
            else
            {
                StatusMessage = $"Failed to request bill for order {orderId}.";
                BillStatusColor = Brushes.Red;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Bill request timed out.";
            BillStatusColor = Brushes.Red;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error requesting bill: {ex.Message}";
            BillStatusColor = Brushes.Red;
        }
    }

    private async Task SyncNowAsync()
    {
        if (_offlineStorage is null)
        {
            StatusMessage = "Sync not available - offline storage not configured.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Synchronizing pending orders...";

        try
        {
            // The SyncManager runs in background, so we just need to wait a bit and refresh
            await Task.Delay(2000); // Give SyncManager time to process
            await RefreshPendingCountAsync();
            await LoadOrdersAsync();
            
            if (PendingSyncCount == 0)
            {
                StatusMessage = "All orders synchronized successfully!";
            }
            else
            {
                StatusMessage = $"{PendingSyncCount} orders still pending synchronization.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during sync: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void StartPendingCountTimer()
    {
        // Timer to refresh pending count every 10 seconds
        var timer = new System.Timers.Timer(10000);
        timer.Elapsed += async (sender, e) =>
        {
            try
            {
                await RefreshPendingCountAsync();
            }
            catch { }
        };
        timer.Start();
    }

    private async Task RefreshPendingCountAsync()
    {
        if (_offlineStorage is not null)
        {
            var count = await _offlineStorage.GetPendingOperationsCountAsync();
            PendingSyncCount = count;
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
