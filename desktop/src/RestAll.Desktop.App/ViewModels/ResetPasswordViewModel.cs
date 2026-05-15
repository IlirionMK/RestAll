using System.Windows.Input;
using System.Windows.Media;
using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.App.ViewModels;

public class ResetPasswordViewModel : CancelableViewModelBase
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    
    private string _token = "";
    private string _email = "";
    private string _password = "";
    private string _passwordConfirmation = "";
    private Brush _statusColor = Brushes.Black;

    public ResetPasswordViewModel(IAuthenticateUserUseCase authUseCase)
    {
        _authUseCase = authUseCase;
        ResetPasswordCommand = new AsyncRelayCommand(ResetPasswordAsync, CanExecuteResetPassword);
    }

    public string Token
    {
        get => _token;
        set => SetProperty(ref _token, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ResetPasswordCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string PasswordConfirmation
    {
        get => _passwordConfirmation;
        set
        {
            if (SetProperty(ref _passwordConfirmation, value))
            {
                ResetPasswordCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Brush StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    public IAsyncRelayCommand ResetPasswordCommand { get; }
    public event EventHandler? PasswordResetSuccessful;

    private bool CanExecuteResetPassword()
    {
        return !IsLoading && 
               !string.IsNullOrWhiteSpace(Password) && 
               Password.Length >= 6 &&
               Password == PasswordConfirmation;
    }

    private async Task ResetPasswordAsync()
    {
        if (Password != PasswordConfirmation)
        {
            StatusMessage = "Passwords do not match.";
            StatusColor = Brushes.Red;
            return;
        }

        if (Password.Length < 6)
        {
            StatusMessage = "Password must be at least 6 characters.";
            StatusColor = Brushes.Red;
            return;
        }

        IsLoading = true;
        StatusMessage = "Resetting password...";
        StatusColor = Brushes.Blue;

        try
        {
            var success = await _authUseCase.ResetPasswordAsync(Email, Token, Password, PasswordConfirmation, GetCancellationToken().Token);

            if (success)
            {
                StatusMessage = "Password has been reset successfully!";
                StatusColor = Brushes.Green;
                PasswordResetSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusMessage = "Failed to reset password. The link may be expired or invalid.";
                StatusColor = Brushes.Red;
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
}
