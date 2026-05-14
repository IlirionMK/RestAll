using Moq;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Integration;

public class AuthenticationIntegrationTests
{
    private readonly Mock<IAuthGateway> _gatewayMock;
    private readonly Mock<ILogger<AuthenticateUserUseCase>> _loggerMock;
    private readonly Mock<ISessionStorage> _sessionStorageMock;
    private readonly AuthenticateUserUseCase _useCase;

    public AuthenticationIntegrationTests()
    {
        _gatewayMock = new Mock<IAuthGateway>();
        _loggerMock = new Mock<ILogger<AuthenticateUserUseCase>>();
        _sessionStorageMock = new Mock<ISessionStorage>();
        _useCase = new AuthenticateUserUseCase(_gatewayMock.Object, _loggerMock.Object, _sessionStorageMock.Object);
    }

    [Fact]
    public async Task FullAuthFlow_ShouldWork()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";
        var session = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");

        _gatewayMock.Setup(g => g.LoginAsync(email, password, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Logged in", session));

        // Act - Login
        var loginResult = await _useCase.LoginAsync(email, password, CancellationToken.None);

        // Assert - Login successful
        loginResult.State.Should().Be(AuthFlowState.Authenticated);
        _useCase.State.Should().Be(AuthFlowState.Authenticated);
        _useCase.CurrentSession.Should().NotBeNull();
        _useCase.CurrentSession!.FullName.Should().Be("John Doe");
        _useCase.CurrentSession.Role.Should().Be("waiter");

        // Arrange - Refresh session
        var newSession = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        _gatewayMock.Setup(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Session refreshed", newSession));

        // Act - Refresh session
        var refreshResult = await _useCase.RefreshTokenAsync(CancellationToken.None);

        // Assert - Session refreshed
        refreshResult.State.Should().Be(AuthFlowState.Authenticated);
        _useCase.CurrentSession!.FullName.Should().Be("John Doe");
        _useCase.CurrentSession.Role.Should().Be("waiter");

        // Arrange - Logout
        _gatewayMock.Setup(g => g.LogoutAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        // Act - Logout
        await _useCase.LogoutAsync(CancellationToken.None);

        // Assert - Logged out
        _useCase.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.CurrentSession.Should().BeNull();

        // Verify all gateway calls
        _gatewayMock.Verify(g => g.LoginAsync(email, password, It.IsAny<CancellationToken>()), Times.Once);
        _gatewayMock.Verify(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        _gatewayMock.Verify(g => g.LogoutAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TokenRefreshFlow_AfterExpiredToken_ShouldHandleGracefully()
    {
        // Arrange
        var session = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        _gatewayMock.Setup(g => g.LoginAsync("test@example.com", "password", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Logged in", session));

        // Act - Login
        await _useCase.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Arrange - Refresh fails
        _gatewayMock.Setup(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Anonymous, "Session expired"));

        // Act - Try to refresh
        var refreshResult = await _useCase.RefreshTokenAsync(CancellationToken.None);

        // Assert - Refresh failed, state reset
        refreshResult.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.State.Should().Be(AuthFlowState.Anonymous);
        _useCase.CurrentSession.Should().BeNull();
    }

    [Fact]
    public async Task MultipleRefreshes_ShouldUpdateSessionCorrectly()
    {
        // Arrange
        var session1 = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        var session2 = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");
        var session3 = new UserSession(string.Empty, string.Empty, "John Doe", "waiter");

        _gatewayMock.Setup(g => g.LoginAsync("test@example.com", "password", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Logged in", session1));

        // Act - Initial login
        await _useCase.LoginAsync("test@example.com", "password", CancellationToken.None);
        _useCase.CurrentSession!.FullName.Should().Be("John Doe");

        // Arrange - First refresh
        _gatewayMock.Setup(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Refreshed", session2));

        // Act - First refresh
        await _useCase.RefreshTokenAsync(CancellationToken.None);
        _useCase.CurrentSession!.FullName.Should().Be("John Doe");

        // Arrange - Second refresh
        _gatewayMock.Setup(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Authenticated, "Refreshed", session3));

        // Act - Second refresh
        await _useCase.RefreshTokenAsync(CancellationToken.None);
        _useCase.CurrentSession!.FullName.Should().Be("John Doe");

        // Verify refresh was called
        _gatewayMock.Verify(g => g.RefreshTokenAsync(string.Empty, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
