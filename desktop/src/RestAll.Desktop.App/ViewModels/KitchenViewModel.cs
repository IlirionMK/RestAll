using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Realtime;

namespace RestAll.Desktop.App.ViewModels;

public class KitchenViewModel : CancelableViewModelBase
{
    private readonly IManageKitchenUseCase _kitchenUseCase;
    private readonly IRealtimeService _realtimeService;
    
    private List<KitchenTicket> _tickets = new();

    public KitchenViewModel(IManageKitchenUseCase kitchenUseCase, IRealtimeService realtimeService)
    {
        _kitchenUseCase = kitchenUseCase;
        _realtimeService = realtimeService;
        _realtimeService.KitchenOrderItemsAdded += OnRealtimeRefreshRequested;
        _realtimeService.KitchenTicketStatusUpdated += OnRealtimeRefreshRequested;
        _realtimeService.ItemReady += OnRealtimeRefreshRequested;
        
        LoadTicketsCommand = new AsyncRelayCommand(LoadTicketsAsync, () => !IsLoading);
        UpdateTicketStatusCommand = new AsyncRelayCommand<int>(ticketId => UpdateTicketStatusAsync(ticketId, OrderItemStatus.Preparing), _ => !IsLoading);
        SetReadyCommand = new AsyncRelayCommand<int>(ticketId => UpdateTicketStatusAsync(ticketId, OrderItemStatus.Ready), _ => !IsLoading);
    }

    public List<KitchenTicket> Tickets
    {
        get => _tickets;
        set => SetProperty(ref _tickets, value);
    }

    public IAsyncRelayCommand LoadTicketsCommand { get; }
    public IAsyncRelayCommand<int> UpdateTicketStatusCommand { get; }
    public IAsyncRelayCommand<int> SetReadyCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadTicketsCommand.NotifyCanExecuteChanged();
        UpdateTicketStatusCommand.NotifyCanExecuteChanged();
        SetReadyCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadTicketsAsync()
    {
        IsLoading = true;
        StatusMessage = "";

        try
        {
            var tickets = await _kitchenUseCase.GetActiveTicketsAsync(GetCancellationToken().Token);
            Tickets = tickets;
            StatusMessage = $"Loaded {tickets.Count} active tickets.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading tickets: {ex.Message}";
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

        _ = LoadTicketsAsync();
    }

    private async Task UpdateTicketStatusAsync(int ticketId, OrderItemStatus status)
    {
        try
        {
            var result = await _kitchenUseCase.UpdateTicketStatusAsync(ticketId, status, GetCancellationToken().Token);
            
            if (result)
            {
                StatusMessage = $"Ticket {ticketId} status updated to {status}.";
                await LoadTicketsAsync();
            }
            else
            {
                StatusMessage = $"Failed to update ticket {ticketId}.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating ticket: {ex.Message}";
        }
    }

    protected override void OnDispose()
    {
        _realtimeService.KitchenOrderItemsAdded -= OnRealtimeRefreshRequested;
        _realtimeService.KitchenTicketStatusUpdated -= OnRealtimeRefreshRequested;
        _realtimeService.ItemReady -= OnRealtimeRefreshRequested;
    }
}
