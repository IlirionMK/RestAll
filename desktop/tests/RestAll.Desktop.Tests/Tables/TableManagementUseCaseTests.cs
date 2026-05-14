using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Tables;
using Xunit;

namespace RestAll.Desktop.Tests.Tables;

public class TableManagementUseCaseTests
{
	private readonly Mock<ITableGateway> _mockGateway;
	private readonly Mock<RestAll.Desktop.Core.Auth.IManageProfileUseCase> _mockProfile;
	private readonly TableManagementUseCase _useCase;

	public TableManagementUseCaseTests()
	{
		_mockGateway = new Mock<ITableGateway>();
		_mockProfile = new Mock<RestAll.Desktop.Core.Auth.IManageProfileUseCase>();
		
		// Default profile returns a restaurant id = 1
		_mockProfile.Setup(p => p.GetProfileAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RestAll.Desktop.Core.Auth.UserProfile(1, "Test", "test@example.com", "user", 1));

		_useCase = new TableManagementUseCase(_mockGateway.Object, _mockProfile.Object);
	}

	[Fact]
	public async Task GetTablesAsync_ShouldReturnTables()
	{
		var expectedTables = new List<Table>
		{
			new Table(1, "Table 1", 4, TableStatus.Available),
			new Table(2, "Table 2", 6, TableStatus.Occupied)
		};

		_mockGateway
			.Setup(g => g.GetTablesAsync(1, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedTables);

		var result = await _useCase.GetTablesAsync(CancellationToken.None);

		result.Should().BeEquivalentTo(expectedTables);
	}

	[Fact]
	public async Task UpdateTableStatusAsync_ShouldReturnTrue()
	{
		_mockGateway
			.Setup(g => g.UpdateTableStatusAsync(It.IsAny<int>(), It.IsAny<TableStatus>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		var result = await _useCase.UpdateTableStatusAsync(1, TableStatus.Occupied, CancellationToken.None);

		result.Should().BeTrue();
	}
}

