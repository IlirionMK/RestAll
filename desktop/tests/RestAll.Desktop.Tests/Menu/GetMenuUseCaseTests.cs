using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Core.Menu;
using Xunit;

namespace RestAll.Desktop.Tests.Menu;

public class GetMenuUseCaseTests
{
    private readonly Mock<IMenuGateway> _mockGateway;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ILogger<GetMenuUseCase>> _mockLogger;
    private readonly GetMenuUseCase _useCase;

    public GetMenuUseCaseTests()
    {
        _mockGateway = new Mock<IMenuGateway>();
        _mockCache = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<GetMenuUseCase>>();
        _useCase = new GetMenuUseCase(_mockGateway.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnCategories()
    {
        // Arrange
        var expectedCategories = new List<MenuCategory>
        {
            new MenuCategory(1, "Appetizers", 1, new List<MenuItem>()),
            new MenuCategory(2, "Main Course", 2, new List<MenuItem>())
        };

        _mockGateway
            .Setup(g => g.GetCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _useCase.GetCategoriesAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedCategories);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnItems()
    {
        // Arrange
        var expectedItems = new List<MenuItem>
        {
            new MenuItem(1, "Burger", "Delicious burger", 15.99m, null, true, 1, "Main Course"),
            new MenuItem(2, "Fries", "Crispy fries", 5.99m, null, true, 1, "Appetizers")
        };

        _mockGateway
            .Setup(g => g.GetItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _useCase.GetItemsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedItems);
    }
}
