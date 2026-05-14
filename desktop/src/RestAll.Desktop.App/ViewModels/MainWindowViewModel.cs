using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    private string _userInfo = "";

    public MainWindowViewModel(IAuthenticateUserUseCase authUseCase)
    {
        _authUseCase = authUseCase;
        _authUseCase.SessionChanged += OnSessionChanged;
        LoadUserInfo();
        
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
    }

    public string UserInfo
    {
        get => _userInfo;
        set => SetProperty(ref _userInfo, value);
    }

    public IAsyncRelayCommand LogoutCommand { get; }

    private void LoadUserInfo()
    {
        if (_authUseCase.CurrentSession is not null)
        {
            UserInfo = $"Logged in as: {_authUseCase.CurrentSession.FullName} ({_authUseCase.CurrentSession.Role})";
        }
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        LoadUserInfo();
    }

    private async Task LogoutAsync()
    {
        await _authUseCase.LogoutAsync(System.Threading.CancellationToken.None);
    }

    public void Dispose()
    {
        _authUseCase.SessionChanged -= OnSessionChanged;
    }
}
