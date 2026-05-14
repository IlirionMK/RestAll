namespace RestAll.Desktop.App.ViewModels;

public abstract class CancelableViewModelBase : ViewModelBase, IDisposable
{
    private CancellationTokenSource? _cts;
    private bool _isLoading = false;
    private string _statusMessage = "";

    internal CancellationTokenSource GetCancellationToken()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        return _cts;
    }

    internal async Task ExecuteWithCancellationAsync(Func<CancellationToken, Task> action, string errorMessage)
    {
        try
        {
            await action(GetCancellationToken().Token);
        }
        catch (Exception ex)
        {
            StatusMessage = $"{errorMessage}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnIsLoadingChanged();
            }
        }
    }

    protected virtual void OnIsLoadingChanged()
    {
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public void Cancel()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    protected virtual void OnDispose()
    {
    }

    public virtual void Dispose()
    {
        Cancel();
        OnDispose();
    }
}
