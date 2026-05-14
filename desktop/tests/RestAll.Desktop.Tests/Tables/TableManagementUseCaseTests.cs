using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Tables;
using Xunit;

namespace RestAll.Desktop.Tests.Tables;

public class TableManagementUseCaseTests
{
	private readonly Mock<ITableGateway> _mockGateway;
		private readonly Mock<IManageProfileUseCase> _mockProfileUseCase;
	private readonly TableManagementUseCase _useCase;

	public TableManagementUseCaseTests()
	{
		_mockGateway = new Mock<ITableGateway>();
			_mockProfileUseCase = new Mock<IManageProfileUseCase>();
			_useCase = new TableManagementUseCase(_mockGateway.Object, _mockProfileUseCase.Object);
	}

	[Fact]
	public async Task GetTablesAsync_ShouldReturnTables()
	{
		var expectedTables = new List<Table>
		{
			new Table(1, "Table 1", 4, TableStatus.Available),
			new Table(2, "Table 2", 6, TableStatus.Occupied)
		};

		_mockProfileUseCase
			.Setup(p => p.GetProfileAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new UserProfile(1, "John Doe", "john@example.com", "waiter", 42));

		_mockGateway
			.Setup(g => g.GetTablesAsync(42, It.IsAny<CancellationToken>()))
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

