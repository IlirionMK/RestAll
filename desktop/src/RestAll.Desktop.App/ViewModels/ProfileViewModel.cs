using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.App.Validation;
using FluentValidation.Results;

namespace RestAll.Desktop.App.ViewModels;

public class ProfileViewModel : CancelableViewModelBase
{
    private readonly IManageProfileUseCase _profileUseCase;
    private readonly IAuthenticateUserUseCase _authUseCase;
    private readonly UserProfileValidator _validator;
    
    private UserProfile? _currentProfile;
    private string _name = "";
    private string _email = "";
    private string _role = "";

    public ProfileViewModel(IManageProfileUseCase profileUseCase, IAuthenticateUserUseCase authUseCase)
    {
        _profileUseCase = profileUseCase;
        _authUseCase = authUseCase;
        _validator = new UserProfileValidator();
        
        LoadProfileCommand = new AsyncRelayCommand(LoadProfileAsync, () => !IsLoading);
        UpdateProfileCommand = new AsyncRelayCommand(UpdateProfileAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Role));
        
        // Auto-load profile when user session changes
        _authUseCase.SessionChanged += async (sender, args) =>
        {
            // Wait a bit to ensure session is fully loaded
            await Task.Delay(100);
            await LoadProfileAsync();
        };
    }

    public UserProfile? CurrentProfile
    {
        get => _currentProfile;
        set => SetProperty(ref _currentProfile, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public string ValidationErrors { get; private set; } = "";

    private ValidationResult ValidateProfile()
    {
        var profile = new UserProfile(
            CurrentProfile?.Id ?? 0,
            Name,
            Email,
            Role
        );

        return _validator.Validate(profile);
    }

    public IAsyncRelayCommand LoadProfileCommand { get; }
    public IAsyncRelayCommand UpdateProfileCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadProfileCommand.NotifyCanExecuteChanged();
        UpdateProfileCommand.NotifyCanExecuteChanged();
    }

    protected override void OnPropertyChanged(string? propertyName)
    {
        base.OnPropertyChanged(propertyName);
        UpdateProfileCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadProfileAsync()
    {
        IsLoading = true;
        StatusMessage = "";

        try
        {
            var profile = await _profileUseCase.GetProfileAsync(GetCancellationToken().Token);
            
            if (profile is not null)
            {
                CurrentProfile = profile;
                Name = profile.Name;
                Email = profile.Email;
                Role = profile.Role;
                StatusMessage = "Profile loaded successfully.";
            }
            else
            {
                StatusMessage = "Failed to load profile.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UpdateProfileAsync()
    {
        var validationResult = ValidateProfile();
        if (!validationResult.IsValid)
        {
            ValidationErrors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            StatusMessage = $"Validation failed: {ValidationErrors}";
            return;
        }

        ValidationErrors = "";
        
        try
        {
            IsLoading = true;
            StatusMessage = "";
            
            var profile = await _profileUseCase.UpdateProfileAsync(Name, Email, Role, GetCancellationToken().Token);
            
            if (profile is not null)
            {
                CurrentProfile = profile;
                StatusMessage = "Profile updated successfully.";
            }
            else
            {
                StatusMessage = "Failed to update profile.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
