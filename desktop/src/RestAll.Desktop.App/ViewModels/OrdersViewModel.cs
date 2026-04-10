using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.App.ViewModels;

public class OrdersViewModel : CancelableViewModelBase
{
    private readonly IManageOrdersUseCase _ordersUseCase;
    
    private List<Order> _orders = new();

    public OrdersViewModel(IManageOrdersUseCase ordersUseCase)
    {
        _ordersUseCase = ordersUseCase;
        
        LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync, () => !IsLoading);
        PayOrderCommand = new AsyncRelayCommand<int>(PayOrderAsync, _ => !IsLoading);
    }

    public List<Order> Orders
    {
        get => _orders;
        set => SetProperty(ref _orders, value);
    }

    public IAsyncRelayCommand LoadOrdersCommand { get; }
    public IAsyncRelayCommand<int> PayOrderCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadOrdersCommand.NotifyCanExecuteChanged();
        PayOrderCommand.NotifyCanExecuteChanged();
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
}
