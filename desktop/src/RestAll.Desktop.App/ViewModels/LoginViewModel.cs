using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Infrastructure.Auth;

namespace RestAll.Desktop.App.ViewModels;

public class LoginViewModel : CancelableViewModelBase
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    private readonly GoogleAuthBrowserService? _googleAuthService;
    
    private string _email = "";
    private string _password = "";
    private string _twoFactorCode = "";
    private bool _isTwoFactorMode = false;
    private Brush _statusColor;
    private string _errorMessage = "";

    public LoginViewModel(IAuthenticateUserUseCase authUseCase, GoogleAuthBrowserService? googleAuthService = null)
    {
        _authUseCase = authUseCase;
        _googleAuthService = googleAuthService;
        _statusColor = Brushes.Red;
        
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanExecuteLogin);
        VerifyTwoFactorCommand = new AsyncRelayCommand(VerifyTwoFactorAsync, CanExecuteVerifyTwoFactor);
        GoogleLoginCommand = new AsyncRelayCommand(GoogleLoginAsync, () => !IsLoading && !IsTwoFactorMode);
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                ValidateEmail();
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ValidatePassword();
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string TwoFactorCode
    {
        get => _twoFactorCode;
        set
        {
            if (SetProperty(ref _twoFactorCode, value))
            {
                VerifyTwoFactorCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsTwoFactorMode
    {
        get => _isTwoFactorMode;
        set => SetProperty(ref _isTwoFactorMode, value);
    }

    public Brush StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public IAsyncRelayCommand LoginCommand { get; }
    public IAsyncRelayCommand VerifyTwoFactorCommand { get; }
    public IAsyncRelayCommand GoogleLoginCommand { get; }
    public event EventHandler? LoginSuccessful;

    protected override void OnIsLoadingChanged()
    {
        LoginCommand.NotifyCanExecuteChanged();
        VerifyTwoFactorCommand.NotifyCanExecuteChanged();
    }

    private bool CanExecuteLogin()
    {
        return !IsLoading && !IsTwoFactorMode && IsValidLoginData();
    }

    private bool CanExecuteVerifyTwoFactor()
    {
        return !IsLoading && IsTwoFactorMode && !string.IsNullOrWhiteSpace(TwoFactorCode);
    }

    private bool IsValidLoginData()
    {
        ValidateEmail();
        ValidatePassword();
        return string.IsNullOrEmpty(ErrorMessage);
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email jest wymagany.";
        }
        else if (!new EmailAddressAttribute().IsValid(Email))
        {
            ErrorMessage = "Nieprawidłowy format email.";
        }
        else
        {
            if (ErrorMessage == "Email jest wymagany." || ErrorMessage == "Nieprawidłowy format email.")
            {
                ErrorMessage = "";
            }
        }
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Hasło jest wymagane.";
        }
        else if (Password.Length < 6)
        {
            ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.";
        }
        else
        {
            if (ErrorMessage == "Hasło jest wymagane." || ErrorMessage == "Hasło musi mieć co najmniej 6 znaków.")
            {
                ErrorMessage = "";
            }
        }
    }

    private async Task LoginAsync()
    {
        if (!IsValidLoginData())
        {
            StatusMessage = ErrorMessage;
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        StatusMessage = "";
        
        var cts = GetCancellationToken();

        try
        {
            if (!IsTwoFactorMode)
            {
                var result = await _authUseCase.LoginAsync(Email, Password, cts.Token);

                if (result.State == AuthFlowState.RequiresTwoFactor)
                {
                    IsTwoFactorMode = true;
                    StatusMessage = result.Message;
                    StatusColor = Brushes.Blue;
                    ErrorMessage = "";
                }
                else if (result.State == AuthFlowState.Authenticated)
                {
                    StatusMessage = result.Message;
                    StatusColor = Brushes.Green;
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    StatusMessage = result.Message;
                    StatusColor = Brushes.Red;
                }
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task VerifyTwoFactorAsync()
    {
        IsLoading = true;
        StatusMessage = "";
        StatusColor = Brushes.Black;
        
        var cts = GetCancellationToken();

        try
        {
            var result = await _authUseCase.VerifyTwoFactorAsync(TwoFactorCode, cts.Token);

            if (result.State == AuthFlowState.Authenticated)
            {
                StatusMessage = result.Message;
                StatusColor = Brushes.Green;
                IsTwoFactorMode = false;
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusMessage = result.Message;
                StatusColor = Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GoogleLoginAsync()
    {
        if (_googleAuthService == null)
        {
            StatusMessage = "Google authentication is not configured. Please contact administrator.";
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        StatusMessage = "Opening browser for Google authentication...";
        StatusColor = Brushes.Blue;

        try
        {
            var success = await _googleAuthService.AuthenticateAsync(GetCancellationToken().Token);

            if (success)
            {
                StatusMessage = "Google authentication successful! Loading user profile...";
                StatusColor = Brushes.Green;

                // After successful Google OAuth, the session cookies are set by the backend
                // Now we need to load the user profile to complete authentication
                await Task.Delay(1000); // Give backend time to process
                
                // Trigger login successful event - the session will be picked up automatically
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusMessage = "Google authentication failed or was cancelled.";
                StatusColor = Brushes.Red;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Google authentication was cancelled.";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Google authentication error: {ex.Message}";
            StatusColor = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
