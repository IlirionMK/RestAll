using Moq;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Integration;

public class ProfileIntegrationTests
{
    private readonly Mock<IProfileGateway> _gatewayMock;
    private readonly Mock<IAuthenticateUserUseCase> _authUseCaseMock;
    private readonly Mock<ILogger<ManageProfileUseCase>> _loggerMock;
    private readonly ManageProfileUseCase _useCase;

    public ProfileIntegrationTests()
    {
        _gatewayMock = new Mock<IProfileGateway>();
        _authUseCaseMock = new Mock<IAuthenticateUserUseCase>();
        _loggerMock = new Mock<ILogger<ManageProfileUseCase>>();
        _useCase = new ManageProfileUseCase(_gatewayMock.Object, _authUseCaseMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task FullProfileFlow_ShouldWork()
    {
        // Arrange
        var originalProfile = new UserProfile(1, "John Doe", "john@example.com", "waiter");
        var updatedProfile = new UserProfile(1, "John Smith", "john.smith@example.com", "waiter");

        _authUseCaseMock.Setup(a => a.State).Returns(AuthFlowState.Authenticated);
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(string.Empty, string.Empty, "John Doe", "waiter"));

        _gatewayMock.Setup(g => g.GetProfileAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(originalProfile);

        _gatewayMock.Setup(g => g.UpdateProfileAsync(string.Empty, "John Smith", "john.smith@example.com", "waiter", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updatedProfile);

        // Act - Get initial profile
        var initialProfile = await _useCase.GetProfileAsync(CancellationToken.None);

        // Assert - Profile retrieved
        initialProfile.Should().NotBeNull();
        initialProfile!.Name.Should().Be("John Doe");

        // Act - Update profile
        var updated = await _useCase.UpdateProfileAsync("John Smith", "john.smith@example.com", "waiter", CancellationToken.None);

        // Assert - Profile updated
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("John Smith");
        updated.Email.Should().Be("john.smith@example.com");
        updated.Role.Should().Be("waiter");

        // Verify gateway calls
        _gatewayMock.Verify(g => g.GetProfileAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        _gatewayMock.Verify(g => g.UpdateProfileAsync(string.Empty, "John Smith", "john.smith@example.com", "waiter", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProfileFlow_ShouldUpdateProfileData()
    {
        // Arrange
        var originalProfile = new UserProfile(1, "John Doe", "john@example.com", "waiter");
        var updatedProfile = new UserProfile(1, "John Smith", "john@example.com", "waiter");

        _authUseCaseMock.Setup(a => a.State).Returns(AuthFlowState.Authenticated);
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(string.Empty, string.Empty, "John Doe", "waiter"));

        _gatewayMock.Setup(g => g.GetProfileAsync(string.Empty, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(originalProfile);

        _gatewayMock.Setup(g => g.UpdateProfileAsync(string.Empty, "John Smith", "john@example.com", "waiter", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updatedProfile);

        // Act - Get initial profile
        var initialProfile = await _useCase.GetProfileAsync(CancellationToken.None);

        // Assert
        initialProfile.Should().NotBeNull();

        // Act - Update profile
        var updated = await _useCase.UpdateProfileAsync("John Smith", "john@example.com", "waiter", CancellationToken.None);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("John Smith");
    }

    [Fact]
    public async Task ProfileFlow_WithoutSession_ShouldFail()
    {
        // Arrange
        _authUseCaseMock.Setup(a => a.State).Returns(AuthFlowState.Anonymous);
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns((UserSession?)null);

        // Act - Try to get profile without session
        var profile = await _useCase.GetProfileAsync(CancellationToken.None);

        // Assert
        profile.Should().BeNull();
        _gatewayMock.Verify(g => g.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // Act - Try to update profile without session
        var updated = await _useCase.UpdateProfileAsync("John Smith", "john@example.com", "waiter", CancellationToken.None);

        // Assert
        updated.Should().BeNull();
        _gatewayMock.Verify(g => g.UpdateProfileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
