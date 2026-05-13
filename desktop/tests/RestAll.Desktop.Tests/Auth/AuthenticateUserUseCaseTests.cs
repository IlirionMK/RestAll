using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Auth;
using Xunit;

namespace RestAll.Desktop.Tests.Auth;

public class AuthenticateUserUseCaseTests
{
    private readonly Mock<IAuthGateway> _mockGateway;
    private readonly AuthenticateUserUseCase _useCase;

    public AuthenticateUserUseCaseTests()
    {
        _mockGateway = new Mock<IAuthGateway>();
        _useCase = new AuthenticateUserUseCase(_mockGateway.Object);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ShouldReturnError()
    {
        // Arrange
        var email = TestData.EmptyString;
        var password = TestData.ValidPassword;

        // Act
        var result = await _useCase.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Be("Please provide email and password.");
        result.Session.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ShouldReturnError()
    {
        // Arrange
        var email = "test@example.com";
        var password = "";

        // Act
        var result = await _useCase.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Be("Please provide email and password.");
        result.Session.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthenticated()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var expectedSession = new UserSession("access_token", "refresh_token", "John Doe", "Admin");
        var expectedResult = new AuthResult(AuthFlowState.Authenticated, "Zalogowano pomyślnie.", expectedSession);

        _mockGateway
            .Setup(g => g.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _useCase.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Authenticated);
        result.Session.Should().NotBeNull();
        result.Session!.FullName.Should().Be("John Doe");
        _useCase.State.Should().Be(AuthFlowState.Authenticated);
        _useCase.CurrentSession.Should().Be(expectedSession);
    }

    [Fact]
    public async Task LoginAsync_WithTwoFactorRequired_ShouldReturnRequiresTwoFactor()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var expectedTicket = "ticket123";
        var expectedResult = new AuthResult(AuthFlowState.RequiresTwoFactor, "Wymagana weryfikacja dwuetapowa.", TwoFactorTicket: expectedTicket);

        _mockGateway
            .Setup(g => g.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _useCase.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.RequiresTwoFactor);
        result.TwoFactorTicket.Should().Be(expectedTicket);
        _useCase.State.Should().Be(AuthFlowState.RequiresTwoFactor);
        _useCase.PendingTwoFactorTicket.Should().Be(expectedTicket);
    }

    [Fact]
    public async Task LoginAsync_WithFailure_ShouldResetState()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrongpassword";
        var expectedResult = new AuthResult(AuthFlowState.Anonymous, "Błąd logowania.");

        _mockGateway
            .Setup(g => g.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _useCase.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.CurrentSession.Should().BeNull();
        _useCase.PendingTwoFactorTicket.Should().BeNull();
    }

    [Fact]
    public async Task VerifyTwoFactorAsync_WithoutActiveTwoFactor_ShouldReturnError()
    {
        // Arrange
        var code = "123456";

        // Act
        var result = await _useCase.VerifyTwoFactorAsync(code, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Be("No active 2FA step.");
    }

    [Fact]
    public async Task VerifyTwoFactorAsync_WithEmptyCode_ShouldReturnError()
    {
        // Arrange
        _mockGateway
            .Setup(g => g.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResult(AuthFlowState.RequiresTwoFactor, "Wymagana weryfikacja dwuetapowa.", TwoFactorTicket: "ticket123"));

        await _useCase.LoginAsync("test@example.com", "password", CancellationToken.None);
        var code = "";

        // Act
        var result = await _useCase.VerifyTwoFactorAsync(code, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.RequiresTwoFactor);
        result.Message.Should().Be("Please enter 2FA code.");
    }

    [Fact]
    public async Task VerifyTwoFactorAsync_WithValidCode_ShouldReturnAuthenticated()
    {
        // Arrange
        var ticket = "ticket123";
        var code = "123456";
        var expectedSession = new UserSession("access_token", "refresh_token", "John Doe", "Admin");
        var expectedResult = new AuthResult(AuthFlowState.Authenticated, "Zalogowano pomyślnie.", expectedSession);

        _mockGateway
            .Setup(g => g.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResult(AuthFlowState.RequiresTwoFactor, "Wymagana weryfikacja dwuetapowa.", TwoFactorTicket: ticket));

        _mockGateway
            .Setup(g => g.VerifyTwoFactorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        await _useCase.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Act
        var result = await _useCase.VerifyTwoFactorAsync(code, CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Authenticated);
        result.Session.Should().NotBeNull();
        _useCase.State.Should().Be(AuthFlowState.Authenticated);
    }

    [Fact]
    public void ResetState_ShouldClearAllState()
    {
        // Arrange
        var expectedSession = new UserSession("access_token", "refresh_token", "John Doe", "Admin");
        _useCase.GetType().GetProperty("State")?.SetValue(_useCase, AuthFlowState.Authenticated);
        _useCase.GetType().GetProperty("CurrentSession")?.SetValue(_useCase, expectedSession);
        _useCase.GetType().GetProperty("PendingTwoFactorTicket")?.SetValue(_useCase, "ticket123");

        // Act
        _useCase.ResetState();

        // Assert
        _useCase.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.CurrentSession.Should().BeNull();
        _useCase.PendingTwoFactorTicket.Should().BeNull();
    }
}
