using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private readonly Mock<IAuthenticateUserUseCase> _mockAuthUseCase;
    private readonly Mock<IRealtimeService> _mockRealtimeService;
    private readonly MainWindowViewModel _viewModel;

    public MainWindowViewModelTests()
    {
        _mockAuthUseCase = new Mock<IAuthenticateUserUseCase>();
        _mockRealtimeService = new Mock<IRealtimeService>();
        _mockRealtimeService.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockRealtimeService.Setup(s => s.DisconnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockRealtimeService.Setup(s => s.IsConnectedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _viewModel = new MainWindowViewModel(_mockAuthUseCase.Object, _mockRealtimeService.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.UserInfo.Should().Be("");
        _viewModel.CanOpenAdminDashboard.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldSubscribeToSessionChanged()
    {
        // Arrange
        var session = new UserSession("access_token", "refresh_token", "John Doe", "Admin");
        _mockAuthUseCase.Setup(u => u.CurrentSession).Returns(session);
        _mockAuthUseCase.Setup(u => u.State).Returns(AuthFlowState.Authenticated);

        // Act
        _mockAuthUseCase.Raise(u => u.SessionChanged += null, EventArgs.Empty);

        // Assert
        _viewModel.UserInfo.Should().Contain("John Doe");
        _viewModel.UserInfo.Should().Contain("Admin");
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromSessionChanged()
    {
        // Arrange
        // Act
        _viewModel.Dispose();

        // Assert
        // Should not throw exception
    }

    [Fact]
    public void SessionChanged_WhenAnonymous_ShouldClearUserInfo()
    {
        // Arrange
        _mockAuthUseCase.Setup(u => u.CurrentSession).Returns((UserSession?)null);
        _mockAuthUseCase.Setup(u => u.State).Returns(AuthFlowState.Anonymous);

        // Act
        _mockAuthUseCase.Raise(u => u.SessionChanged += null, EventArgs.Empty);

        // Assert
        _viewModel.UserInfo.Should().Be("");
        _viewModel.CanOpenAdminDashboard.Should().BeFalse();
    }

    [Fact]
    public void SessionChanged_WhenAdmin_ShouldEnableAdminDashboard()
    {
        // Arrange
        var session = new UserSession("access_token", "refresh_token", "Jane Smith", "Admin");
        _mockAuthUseCase.Setup(u => u.CurrentSession).Returns(session);
        _mockAuthUseCase.Setup(u => u.State).Returns(AuthFlowState.Authenticated);

        // Act
        _mockAuthUseCase.Raise(u => u.SessionChanged += null, EventArgs.Empty);

        // Assert
        _viewModel.CanOpenAdminDashboard.Should().BeTrue();
    }

    [Fact]
    public void SessionChanged_WhenAuthenticated_ShouldUpdateUserInfo()
    {
        // Arrange
        var session = new UserSession("access_token", "refresh_token", "Jane Smith", "Manager");
        _mockAuthUseCase.Setup(u => u.CurrentSession).Returns(session);
        _mockAuthUseCase.Setup(u => u.State).Returns(AuthFlowState.Authenticated);

        // Act
        _mockAuthUseCase.Raise(u => u.SessionChanged += null, EventArgs.Empty);

        // Assert
        _viewModel.UserInfo.Should().Contain("Jane Smith");
        _viewModel.UserInfo.Should().Contain("Manager");
    }
}
