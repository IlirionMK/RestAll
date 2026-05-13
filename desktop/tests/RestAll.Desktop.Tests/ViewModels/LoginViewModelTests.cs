using System.Windows.Media;
using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class LoginViewModelTests
{
    private readonly Mock<IAuthenticateUserUseCase> _mockAuthUseCase;
    private readonly LoginViewModel _viewModel;

    public LoginViewModelTests()
    {
        _mockAuthUseCase = new Mock<IAuthenticateUserUseCase>();
        _viewModel = new LoginViewModel(_mockAuthUseCase.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Email.Should().Be("");
        _viewModel.Password.Should().Be("");
        _viewModel.TwoFactorCode.Should().Be("");
        _viewModel.StatusMessage.Should().Be("");
        _viewModel.IsTwoFactorMode.Should().BeFalse();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.StatusColor.Should().Be(Brushes.Red);
        _viewModel.LoginCommand.Should().NotBeNull();
    }

    [Fact]
    public void LoginCommand_WithoutValidCredentials_ShouldNotBeExecutable()
    {
        // Arrange
        var command = _viewModel.LoginCommand;

        // Act
        _viewModel.IsLoading = true;

        // Assert
        command.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void LoginCommand_WithValidCredentials_ShouldRespectIsLoadingState()
    {
        // Arrange
        var command = _viewModel.LoginCommand;

        _viewModel.Email = TestData.ValidEmail;
        _viewModel.Password = TestData.ValidPassword;

        command.CanExecute(null).Should().BeTrue();

        // Act
        _viewModel.IsLoading = true;

        // Assert
        command.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_WithTwoFactorRequired_ShouldSwitchToTwoFactorMode()
    {
        // Arrange
        _viewModel.Email = TestData.ValidEmail;
        _viewModel.Password = TestData.ValidPassword;
        var expectedResult = new AuthResult(AuthFlowState.RequiresTwoFactor, "Wymagana weryfikacja dwuetapowa.", TwoFactorTicket: TestData.TwoFactorTicket);

        _mockAuthUseCase
            .Setup(u => u.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync();

        // Assert
        _viewModel.IsTwoFactorMode.Should().BeTrue();
        _viewModel.StatusMessage.Should().Be("Wymagana weryfikacja dwuetapowa.");
        _viewModel.StatusColor.Should().Be(Brushes.Blue);
    }

    [Fact]
    public async Task LoginCommand_WithAuthenticated_ShouldRaiseLoginSuccessfulEvent()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        var expectedSession = new UserSession("access_token", "refresh_token", "John Doe", "Admin");
        var expectedResult = new AuthResult(AuthFlowState.Authenticated, "Zalogowano pomyślnie.", expectedSession);

        _mockAuthUseCase
            .Setup(u => u.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var eventRaised = false;
        _viewModel.LoginSuccessful += (s, e) => eventRaised = true;

        // Act
        await _viewModel.LoginCommand.ExecuteAsync();

        // Assert
        eventRaised.Should().BeTrue();
        _viewModel.StatusMessage.Should().Be("Zalogowano pomyślnie.");
        _viewModel.StatusColor.Should().Be(Brushes.Green);
    }

    [Fact]
    public async Task LoginCommand_WithError_ShouldShowErrorMessage()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "wrongpassword";
        var expectedResult = new AuthResult(AuthFlowState.Anonymous, "Błąd logowania.");

        _mockAuthUseCase
            .Setup(u => u.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync();

        // Assert
        _viewModel.StatusMessage.Should().Be("Błąd logowania.");
        _viewModel.StatusColor.Should().Be(Brushes.Red);
    }

    [Fact]
    public void Cancel_ShouldCancelCancellationToken()
    {
        // Arrange
        // Act
        _viewModel.Cancel();

        // Assert
        // Should not throw exception (Cancel is inherited from CancelableViewModelBase)
    }
}
