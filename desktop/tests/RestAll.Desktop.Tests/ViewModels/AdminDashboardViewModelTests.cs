using FluentAssertions;
using Moq;
using RestAll.Desktop.App.ViewModels;
using RestAll.Desktop.Core.Admin;
using RestAll.Desktop.Core.Auth;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class AdminDashboardViewModelTests
{
    private readonly Mock<IManageStaffUseCase> _mockStaffUseCase;
    private readonly Mock<IGetAnalyticsSummaryUseCase> _mockAnalyticsUseCase;
    private readonly Mock<IGetAuditLogsUseCase> _mockLogsUseCase;
    private readonly AdminDashboardViewModel _viewModel;

    public AdminDashboardViewModelTests()
    {
        _mockStaffUseCase = new Mock<IManageStaffUseCase>();
        _mockAnalyticsUseCase = new Mock<IGetAnalyticsSummaryUseCase>();
        _mockLogsUseCase = new Mock<IGetAuditLogsUseCase>();
        
        _viewModel = new AdminDashboardViewModel(
            _mockStaffUseCase.Object,
            _mockAnalyticsUseCase.Object,
            _mockLogsUseCase.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        _viewModel.StaffMembers.Should().NotBeNull();
        _viewModel.AvailableRoles.Should().Contain(new[] { "admin", "manager", "waiter", "chef" });
        _viewModel.NewStaffRole.Should().Be("waiter");
        _viewModel.AuditLogPerPage.Should().Be("25");
    }

    [Fact]
    public async Task LoadStaffCommand_ShouldPopulateStaffMembers()
    {
        // Arrange
        var staff = new List<UserProfile>
        {
            new UserProfile(1, "Admin", "admin@example.com", "admin"),
            new UserProfile(2, "Waiter", "waiter@example.com", "waiter")
        };
        _mockStaffUseCase.Setup(u => u.GetStaffAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        // Act
        _viewModel.LoadStaffCommand.Execute(null);
        await Task.Delay(50); // Small delay for async command

        // Assert
        _viewModel.StaffMembers.Should().HaveCount(2);
        _viewModel.StaffMembers[0].Name.Should().Be("Admin");
        _viewModel.StaffStatusMessage.Should().Contain("Loaded 2");
    }

    [Fact]
    public async Task CreateStaffCommand_WithValidData_ShouldAddUser()
    {
        // Arrange
        _viewModel.NewStaffName = "New Guy";
        _viewModel.NewStaffEmail = "new@example.com";
        _viewModel.NewStaffPassword = "password";
        _viewModel.NewStaffRole = "chef";

        var newUser = new UserProfile(3, "New Guy", "new@example.com", "chef");
        _mockStaffUseCase.Setup(u => u.CreateStaffAsync("New Guy", "new@example.com", "password", "chef", It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUser);

        // Act
        _viewModel.CreateStaffCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        _viewModel.StaffMembers.Should().Contain(newUser);
        _viewModel.NewStaffName.Should().Be("");
        _viewModel.StaffStatusMessage.Should().Contain("created successfully");
    }

    [Fact]
    public async Task UpdateSelectedStaffRoleCommand_ShouldUpdateRole()
    {
        // Arrange
        var user = new UserProfile(1, "Admin", "admin@example.com", "admin");
        _viewModel.StaffMembers.Add(user);
        _viewModel.SelectedStaffMember = user;
        _viewModel.SelectedStaffRole = "manager";

        var updatedUser = user with { Role = "manager" };
        _mockStaffUseCase.Setup(u => u.UpdateStaffRoleAsync(1, "manager", It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        _viewModel.UpdateSelectedStaffRoleCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        _viewModel.SelectedStaffMember!.Role.Should().Be("manager");
        _viewModel.StaffStatusMessage.Should().Contain("updated successfully");
    }

    [Fact]
    public async Task LoadAnalyticsCommand_ShouldPopulateSummary()
    {
        // Arrange
        var summary = new AnalyticsSummary(
            new AnalyticsRevenueStats(100, 500, 2000),
            new AnalyticsOrderStats(5, 25, 100, 20),
            new List<AnalyticsTopItem>(),
            new AnalyticsReservationStats(2, 10)
        );
        _mockAnalyticsUseCase.Setup(u => u.GetSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        _viewModel.LoadAnalyticsCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        _viewModel.AnalyticsSummary.Should().NotBeNull();
        _viewModel.AnalyticsSummary!.Revenue.Today.Should().Be(100);
        _viewModel.AnalyticsStatusMessage.Should().Contain("Analytics loaded");
    }

    [Fact]
    public async Task LoadLogsCommand_ShouldPopulateLogs()
    {
        // Arrange
        var page = new AuditLogPage(new List<AuditLogEntry>(), 1, 1, 0, 25);
        _mockLogsUseCase.Setup(u => u.GetLogsAsync(It.IsAny<AuditLogQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        // Act
        _viewModel.LoadLogsCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        _viewModel.AuditLogPage.Should().NotBeNull();
        _viewModel.LogsStatusMessage.Should().Contain("Loaded 0 logs");
    }
}
