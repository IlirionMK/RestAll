using Moq;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Auth;

public class ManageProfileUseCaseTests
{
    private readonly Mock<IProfileGateway> _gatewayMock;
    private readonly Mock<IAuthenticateUserUseCase> _authUseCaseMock;
    private readonly Mock<ILogger<ManageProfileUseCase>> _loggerMock;
    private readonly ManageProfileUseCase _useCase;

    public ManageProfileUseCaseTests()
    {
        _gatewayMock = new Mock<IProfileGateway>();
        _authUseCaseMock = new Mock<IAuthenticateUserUseCase>();
        _loggerMock = new Mock<ILogger<ManageProfileUseCase>>();
        _useCase = new ManageProfileUseCase(_gatewayMock.Object, _authUseCaseMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnProfile()
    {
        // Arrange
        var expectedProfile = new UserProfile(1, "John Doe", "john@example.com", "waiter");
        _authUseCaseMock.Setup(a => a.State).Returns(AuthFlowState.Authenticated);
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(string.Empty, string.Empty, "John Doe", "waiter"));
        _gatewayMock.Setup(g => g.GetProfileAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedProfile);

        // Act
        var result = await _useCase.GetProfileAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedProfile);
        _gatewayMock.Verify(g => g.GetProfileAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnNull_WhenNoSession()
    {
        // Arrange
        _authUseCaseMock.Setup(a => a.State).Returns(AuthFlowState.Anonymous);
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns((UserSession?)null);

        // Act
        var result = await _useCase.GetProfileAsync(CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _gatewayMock.Verify(g => g.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnUpdatedProfile()
    {
        // Arrange
        var updatedProfile = new UserProfile(1, "John Smith", "john@example.com", "waiter");
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(string.Empty, string.Empty, "John Doe", "waiter"));
        _gatewayMock.Setup(g => g.UpdateProfileAsync(string.Empty, "John Smith", "john@example.com", "waiter", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updatedProfile);

        // Act
        var result = await _useCase.UpdateProfileAsync("John Smith", "john@example.com", "waiter", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedProfile);
        _gatewayMock.Verify(g => g.UpdateProfileAsync(string.Empty, "John Smith", "john@example.com", "waiter", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenNoSession()
    {
        // Arrange
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns((UserSession?)null);

        // Act
        var result = await _useCase.UpdateProfileAsync("John Smith", "john@example.com", "waiter", CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _gatewayMock.Verify(g => g.UpdateProfileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldCallGatewayWithExpectedPayload()
    {
        // Arrange
        var updatedProfile = new UserProfile(1, "John Smith", "john@example.com", "waiter");
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(string.Empty, string.Empty, "John Doe", "waiter"));
        _gatewayMock.Setup(g => g.UpdateProfileAsync(string.Empty, "John Smith", "john@example.com", "waiter", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updatedProfile);

        // Act
        var result = await _useCase.UpdateProfileAsync("John Smith", "john@example.com", "waiter", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedProfile);
        _gatewayMock.Verify(g => g.UpdateProfileAsync(string.Empty, "John Smith", "john@example.com", "waiter", It.IsAny<CancellationToken>()), Times.Once);
    }
}
