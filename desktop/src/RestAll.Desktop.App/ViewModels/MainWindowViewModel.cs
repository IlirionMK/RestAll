using System.Windows;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Realtime;

namespace RestAll.Desktop.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    private readonly IRealtimeService _realtimeService;
    private string _userInfo = "";
    private bool _canOpenAdminDashboard;
    private bool _canOpenMenuManagement;

    public event EventHandler? LogoutRequested;

    public MainWindowViewModel(IAuthenticateUserUseCase authUseCase, IRealtimeService realtimeService)
    {
        _authUseCase = authUseCase;
        _realtimeService = realtimeService;
        _authUseCase.SessionChanged += OnSessionChanged;
        LoadUserInfo();
        _ = SyncRealtimeStateAsync();
        
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
        
        // Listen for session changes to handle logout redirect
        _authUseCase.SessionChanged += async (s, e) =>
        {
            if (_authUseCase.State == AuthFlowState.Anonymous)
            {
                // Session ended - redirect to login
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var app = (App)Application.Current;
                    var loginView = app.CreateLoginView();
                    
                    // Close current main window
                    if (Application.Current.MainWindow is not null)
                    {
                        Application.Current.MainWindow.Close();
                    }
                    
                    // Show login view
                    loginView.Show();
                    Application.Current.MainWindow = loginView;
                });
            }
        };
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

    public bool CanOpenMenuManagement
    {
        get => _canOpenMenuManagement;
        set => SetProperty(ref _canOpenMenuManagement, value);
    }

    public IAsyncRelayCommand LogoutCommand { get; }

    private void LoadUserInfo()
    {
        if (_authUseCase.CurrentSession is not null)
        {
            UserInfo = $"Logged in as: {_authUseCase.CurrentSession.FullName} ({_authUseCase.CurrentSession.Role})";
            CanOpenAdminDashboard = _authUseCase.CurrentSession.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
            CanOpenMenuManagement = _authUseCase.CurrentSession.Role.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                                    _authUseCase.CurrentSession.Role.Equals("manager", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            UserInfo = string.Empty;
            CanOpenAdminDashboard = false;
            CanOpenMenuManagement = false;
        }
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        LoadUserInfo();
        _ = SyncRealtimeStateAsync();
        
        // If session ended (logout), notify view to redirect to login
        if (_authUseCase.State == AuthFlowState.Anonymous)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
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
