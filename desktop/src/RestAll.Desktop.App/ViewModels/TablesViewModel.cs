using RestAll.Desktop.Core.Tables;

namespace RestAll.Desktop.App.ViewModels;

public class TablesViewModel : CancelableViewModelBase
{
    private readonly ITableManagementUseCase _tablesUseCase;
    
    private List<Table> _tables = new();

    public TablesViewModel(ITableManagementUseCase tablesUseCase)
    {
        _tablesUseCase = tablesUseCase;
        
        LoadTablesCommand = new AsyncRelayCommand(LoadTablesAsync, () => !IsLoading);
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

    private async Task LoadTablesAsync()
    {
        IsLoading = true;
        StatusMessage = "";

        try
        {
            var tables = await _tablesUseCase.GetTablesAsync(GetCancellationToken().Token);
            Tables = tables;
            StatusMessage = $"Loaded {tables.Count} tables.";
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
        try
        {
            var result = await _tablesUseCase.UpdateTableStatusAsync(tableId, status, GetCancellationToken().Token);
            
            if (result)
            {
                StatusMessage = $"Table {tableId} status updated to {status}.";
                await LoadTablesAsync();
            }
            else
            {
                StatusMessage = $"Failed to update table {tableId}.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating table status: {ex.Message}";
        }
    }
}
