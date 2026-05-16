using RestAll.Desktop.Core.Tables;

namespace RestAll.Desktop.App.ViewModels;

public class TablesViewModel : CancelableViewModelBase
{
    private readonly ITableManagementUseCase _tablesUseCase;
    
    private List<Table> _tables = new();

    public TablesViewModel(ITableManagementUseCase tablesUseCase)
    {
        _tablesUseCase = tablesUseCase;
        
        LoadTablesCommand = new AsyncRelayCommand(async () => await LoadTablesAsync(), () => !IsLoading);
        UpdateTableStatusCommand = new AsyncRelayCommand<int, TableStatus>(UpdateTableStatusAsync, (_, _) => !IsLoading);
    }

    public List<Table> Tables
    {
        get => _tables;
        set => SetProperty(ref _tables, value);
    }

    public IAsyncRelayCommand LoadTablesCommand { get; }
    public IAsyncRelayCommand<int, TableStatus> UpdateTableStatusCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadTablesCommand.NotifyCanExecuteChanged();
        UpdateTableStatusCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadTablesAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        StatusMessage = "";

        try
        {
            // Use provided token or create a new one if none provided
            var token = cancellationToken != default ? cancellationToken : GetCancellationToken().Token;
            var tables = await _tablesUseCase.GetTablesAsync(token);
            Tables = tables;
            StatusMessage = $"Loaded {tables.Count} tables.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Loading tables was cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading tables: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UpdateTableStatusAsync(int tableId, TableStatus status)
    {
        // Use a dedicated CancellationToken for this operation to avoid cancellation from profile refreshes
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        try
        {
            var result = await _tablesUseCase.UpdateTableStatusAsync(tableId, status, cts.Token);
            
            if (result)
            {
                StatusMessage = $"Table {tableId} status updated to {status}.";
                
                // Use separate token for refresh to avoid cancelling the refresh itself
                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await LoadTablesAsync(refreshCts.Token);
            }
            else
            {
                StatusMessage = $"Failed to update table {tableId}.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = $"Table status update timed out.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating table status: {ex.Message}";
        }
    }
}
