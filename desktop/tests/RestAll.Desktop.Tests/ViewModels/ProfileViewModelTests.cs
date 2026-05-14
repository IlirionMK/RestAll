using Moq;
using RestAll.Desktop.App.ViewModels;
using RestAll.Desktop.Core.Auth;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class ProfileViewModelTests
{
    private readonly Mock<IManageProfileUseCase> _useCaseMock;
    private readonly Mock<IAuthenticateUserUseCase> _authUseCaseMock;
    private readonly ProfileViewModel _viewModel;

    public ProfileViewModelTests()
    {
        _useCaseMock = new Mock<IManageProfileUseCase>();
        _authUseCaseMock = new Mock<IAuthenticateUserUseCase>();
        _viewModel = new ProfileViewModel(_useCaseMock.Object, _authUseCaseMock.Object);
    }

    [Fact]
    public void LoadProfileCommand_ShouldBeExecutable_WhenNotLoading()
    {
        // Arrange
        _viewModel.IsLoading = false;

        // Act & Assert
        _viewModel.LoadProfileCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void LoadProfileCommand_ShouldNotBeExecutable_WhenLoading()
    {
        // Arrange
        _viewModel.IsLoading = true;

        // Act & Assert
        _viewModel.LoadProfileCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task LoadProfileAsync_ShouldLoadProfile()
    {
        // Arrange
        var expectedProfile = new UserProfile(1, "John Doe", "john@example.com", "waiter");
        _useCaseMock.Setup(u => u.GetProfileAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedProfile);

        // Act
        await _viewModel.LoadProfileCommand.ExecuteAsync();

        // Assert
        _viewModel.CurrentProfile.Should().NotBeNull();
        _viewModel.CurrentProfile.Should().BeEquivalentTo(expectedProfile);
        _viewModel.Name.Should().Be("John Doe");
        _viewModel.Email.Should().Be("john@example.com");
        _viewModel.Role.Should().Be("waiter");
        _viewModel.StatusMessage.Should().Contain("Profile loaded successfully");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProfileAsync_ShouldShowError_WhenLoadFails()
    {
        // Arrange
        _useCaseMock.Setup(u => u.GetProfileAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync((UserProfile?)null);

        // Act
        await _viewModel.LoadProfileCommand.ExecuteAsync();

        // Assert
        _viewModel.CurrentProfile.Should().BeNull();
        _viewModel.StatusMessage.Should().Contain("Failed to load profile");
    }

    [Fact]
    public void UpdateProfileCommand_ShouldNotBeExecutable_WhenNameEmpty()
    {
        // Arrange
        _viewModel.Name = "";

        // Act & Assert
        _viewModel.UpdateProfileCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void UpdateProfileCommand_ShouldBeExecutable_WhenNameNotEmpty()
    {
        // Arrange
        _viewModel.Name = "John Doe";
        _viewModel.Email = "john@example.com";
        _viewModel.Role = "waiter";

        // Act & Assert
        _viewModel.UpdateProfileCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateProfile()
    {
        // Arrange
        var currentProfile = new UserProfile(1, "John Doe", "john@example.com", "waiter");
        var updatedProfile = new UserProfile(1, "John Smith", "john.smith@example.com", "manager");
        _viewModel.CurrentProfile = currentProfile;
        _viewModel.Name = "John Smith";
        _viewModel.Email = "john.smith@example.com";
        _viewModel.Role = "manager";
        
        _useCaseMock.Setup(u => u.UpdateProfileAsync(
                "John Smith",
                "john.smith@example.com",
                "manager",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProfile);

        // Act
        await _viewModel.UpdateProfileCommand.ExecuteAsync();

        // Assert
        _viewModel.CurrentProfile.Should().NotBeNull();
        _viewModel.CurrentProfile!.Name.Should().Be("John Smith");
        _viewModel.CurrentProfile.Email.Should().Be("john.smith@example.com");
        _viewModel.CurrentProfile.Role.Should().Be("manager");
        _viewModel.StatusMessage.Should().Contain("Profile updated successfully");
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldShowError_WhenUpdateFails()
    {
        // Arrange
        var currentProfile = new UserProfile(1, "John Doe", "john@example.com", "waiter");
        _viewModel.CurrentProfile = currentProfile;
        _viewModel.Name = "John Smith";
        _viewModel.Email = "john.smith@example.com";
        _viewModel.Role = "manager";
        
        _useCaseMock.Setup(u => u.UpdateProfileAsync(
                "John Smith",
                "john.smith@example.com",
                "manager",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        // Act
        await _viewModel.UpdateProfileCommand.ExecuteAsync();

        // Assert
        _viewModel.StatusMessage.Should().Contain("Failed to update profile");
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldCallUseCaseWithProfileData()
    {
        // Arrange
        var currentProfile = new UserProfile(1, "John Doe", "john@example.com", "waiter");
        var updatedProfile = new UserProfile(1, "John Smith", "john@example.com", "waiter");
        _viewModel.CurrentProfile = currentProfile;
        _viewModel.Name = "John Smith";
        _viewModel.Email = "john.smith@example.com";
        _viewModel.Role = "waiter";
        
        _useCaseMock.Setup(u => u.UpdateProfileAsync(
                "John Smith",
                "john.smith@example.com",
                "waiter",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProfile);

        // Act
        await _viewModel.UpdateProfileCommand.ExecuteAsync();

        // Assert
        _useCaseMock.Verify(u => u.UpdateProfileAsync(
            "John Smith",
            "john.smith@example.com",
            "waiter",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
