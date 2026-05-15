using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Windows.Media;
using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.App.ViewModels;

public class ForgotPasswordViewModel : CancelableViewModelBase
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    
    private string _email = "";
    private Brush _statusColor = Brushes.Black;

    public ForgotPasswordViewModel(IAuthenticateUserUseCase authUseCase)
    {
        _authUseCase = authUseCase;
        SendResetLinkCommand = new AsyncRelayCommand(SendResetLinkAsync, CanExecuteSendResetLink);
        BackToLoginCommand = new RelayCommand(() => BackToLoginRequested?.Invoke(this, EventArgs.Empty), () => true);
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                ValidateEmail();
                SendResetLinkCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Brush StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    public IAsyncRelayCommand SendResetLinkCommand { get; }
    public ICommand BackToLoginCommand { get; }
    public event EventHandler? ResetLinkSent;
    public event EventHandler? BackToLoginRequested;

    private bool CanExecuteSendResetLink()
    {
        return !IsLoading && IsValidEmail();
    }

    private bool IsValidEmail()
    {
        return !string.IsNullOrWhiteSpace(Email) && new EmailAddressAttribute().IsValid(Email);
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            StatusMessage = "Email is required.";
            StatusColor = Brushes.Red;
        }
        else if (!new EmailAddressAttribute().IsValid(Email))
        {
            StatusMessage = "Invalid email format.";
            StatusColor = Brushes.Red;
        }
        else
        {
            StatusMessage = "";
            StatusColor = Brushes.Black;
        }
    }

    private async Task SendResetLinkAsync()
    {
        if (!IsValidEmail())
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Sending reset link...";
        StatusColor = Brushes.Blue;

        try
        {
            var success = await _authUseCase.SendPasswordResetLinkAsync(Email.Trim(), GetCancellationToken().Token);

            if (success)
            {
                StatusMessage = "Password reset link has been sent to your email!";
                StatusColor = Brushes.Green;
                ResetLinkSent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusMessage = "Failed to send reset link. Please try again.";
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
