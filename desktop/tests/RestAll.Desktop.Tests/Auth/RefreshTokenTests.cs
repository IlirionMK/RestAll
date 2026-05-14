using Moq;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Auth;

public class RefreshTokenTests
{
    private readonly Mock<IAuthGateway> _gatewayMock;
    private readonly Mock<ILogger<AuthenticateUserUseCase>> _loggerMock;
    private readonly Mock<ISessionStorage> _sessionStorageMock;
    private readonly AuthenticateUserUseCase _useCase;

    public RefreshTokenTests()
    {
        _gatewayMock = new Mock<IAuthGateway>();
        _loggerMock = new Mock<ILogger<AuthenticateUserUseCase>>();
        _sessionStorageMock = new Mock<ISessionStorage>();
        _useCase = new AuthenticateUserUseCase(_gatewayMock.Object, _loggerMock.Object, _sessionStorageMock.Object);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewSession_WhenRefreshSucceeds()
    {
        // Arrange
        var currentSession = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        var newSession = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        
        // First login to set up session
        _gatewayMock.Setup(g => g.LoginAsync("test@example.com", "password", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Logged in", currentSession));
        await _useCase.LoginAsync("test@example.com", "password", CancellationToken.None);
        
        // Now set up refresh - backend uses cookie-based auth, so we pass empty string
        _gatewayMock.Setup(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Session refreshed", newSession));

        // Act
        var result = await _useCase.RefreshTokenAsync(CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Authenticated);
        _useCase.CurrentSession.Should().NotBeNull();
        _gatewayMock.Verify(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnAnonymous_WhenNoSession()
    {
        // Arrange
        _gatewayMock.Setup(g => g.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Anonymous, "No session"));

        // Act
        var result = await _useCase.RefreshTokenAsync(CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Be("No active session.");
        _gatewayMock.Verify(g => g.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnAnonymous_WhenRefreshFails()
    {
        // Arrange
        var currentSession = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        
        // First login to set up session
        _gatewayMock.Setup(g => g.LoginAsync("test@example.com", "password", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Logged in", currentSession));
        await _useCase.LoginAsync("test@example.com", "password", CancellationToken.None);
        
        // Now set up refresh failure - backend uses cookie-based auth, so we pass empty string
        _gatewayMock.Setup(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Anonymous, "Session expired"));

        // Act
        var result = await _useCase.RefreshTokenAsync(CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.CurrentSession.Should().BeNull();
        _gatewayMock.Verify(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldResetState_WhenRefreshFails()
    {
        // Arrange
        var currentSession = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        
        // First login to set up session
        _gatewayMock.Setup(g => g.LoginAsync("test@example.com", "password", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Logged in", currentSession));
        await _useCase.LoginAsync("test@example.com", "password", CancellationToken.None);
        
        // Now set up refresh failure - backend uses cookie-based auth, so we pass empty string
        _gatewayMock.Setup(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Anonymous, "Session expired"));

        // Act
        await _useCase.RefreshTokenAsync(CancellationToken.None);

        // Assert
        _useCase.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.CurrentSession.Should().BeNull();
    }
}
