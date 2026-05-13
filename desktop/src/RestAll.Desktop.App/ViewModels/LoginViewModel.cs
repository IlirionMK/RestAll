using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.App.ViewModels;

public class LoginViewModel : CancelableViewModelBase
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    
    private string _email = "";
    private string _password = "";
    private string _twoFactorCode = "";
    private bool _isTwoFactorMode = false;
    private Brush _statusColor;
    private string _errorMessage = "";

    public LoginViewModel(IAuthenticateUserUseCase authUseCase)
    {
        _authUseCase = authUseCase;
        _statusColor = Brushes.Red;
        
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanExecuteLogin);
        VerifyTwoFactorCommand = new AsyncRelayCommand(VerifyTwoFactorAsync, CanExecuteVerifyTwoFactor);
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
}
