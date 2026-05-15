using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Realtime;

namespace RestAll.Desktop.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    private readonly IRealtimeService _realtimeService;
    private string _userInfo = "";
    private bool _canOpenAdminDashboard;

    public MainWindowViewModel(IAuthenticateUserUseCase authUseCase, IRealtimeService realtimeService)
    {
        _authUseCase = authUseCase;
        _realtimeService = realtimeService;
        _authUseCase.SessionChanged += OnSessionChanged;
        LoadUserInfo();
        _ = SyncRealtimeStateAsync();
        
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
    }

    public string UserInfo
    {
        get => _userInfo;
        set => SetProperty(ref _userInfo, value);
    }

    public bool CanOpenAdminDashboard
    {
        get => _canOpenAdminDashboard;
        set => SetProperty(ref _canOpenAdminDashboard, value);
    }

    public IAsyncRelayCommand LogoutCommand { get; }

    private void LoadUserInfo()
    {
        if (_authUseCase.CurrentSession is not null)
        {
            UserInfo = $"Logged in as: {_authUseCase.CurrentSession.FullName} ({_authUseCase.CurrentSession.Role})";
            CanOpenAdminDashboard = _authUseCase.CurrentSession.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            UserInfo = string.Empty;
            CanOpenAdminDashboard = false;
        }
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        LoadUserInfo();
        _ = SyncRealtimeStateAsync();
    }

    private async Task SyncRealtimeStateAsync()
    {
        if (_authUseCase.State == AuthFlowState.Authenticated && _authUseCase.CurrentSession is not null)
        {
            await _realtimeService.ConnectAsync();
        }
        else
        {
            await _realtimeService.DisconnectAsync();
        }
    }

    private async Task LogoutAsync()
    {
        await _realtimeService.DisconnectAsync();
        await _authUseCase.LogoutAsync(System.Threading.CancellationToken.None);
    }

    public void Dispose()
    {
        _authUseCase.SessionChanged -= OnSessionChanged;
        _ = _realtimeService.DisconnectAsync();
    }
}
