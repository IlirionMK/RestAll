using Moq;
using RestAll.Desktop.App.ViewModels;
using RestAll.Desktop.Core.Reservations;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class ReservationsViewModelTests
{
    private readonly Mock<IManageReservationsUseCase> _useCaseMock;
    private readonly ReservationsViewModel _viewModel;

    public ReservationsViewModelTests()
    {
        _useCaseMock = new Mock<IManageReservationsUseCase>();
        _viewModel = new ReservationsViewModel(_useCaseMock.Object);
    }

    [Fact]
    public void LoadReservationsCommand_ShouldBeExecutable_WhenNotLoading()
    {
        // Arrange
        _viewModel.IsLoading = false;

        // Act & Assert
        _viewModel.LoadReservationsCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void LoadReservationsCommand_ShouldNotBeExecutable_WhenLoading()
    {
        // Arrange
        _viewModel.IsLoading = true;

        // Act & Assert
        _viewModel.LoadReservationsCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task LoadReservationsAsync_ShouldLoadReservations()
    {
        // Arrange
        var date = DateTime.Now;
        var expectedReservations = new List<Reservation>
        {
            new Reservation(1, "John Doe", "123456789", "john@example.com", date, date.AddHours(18), 1, 4, "confirmed", null)
        };
        _viewModel.SelectedDate = date;
        _useCaseMock.Setup(u => u.GetReservationsForDateAsync(date, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedReservations);

        // Act
        await _viewModel.LoadReservationsCommand.ExecuteAsync();

        // Assert
        _viewModel.Reservations.Should().HaveCount(1);
        _viewModel.StatusMessage.Should().Contain("Loaded 1 reservations");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void CreateReservationCommand_ShouldNotBeExecutable_WhenDataInvalid()
    {
        // Arrange
        _viewModel.CustomerName = "";
        _viewModel.CustomerPhone = "123456789";
        _viewModel.CustomerEmail = "test@example.com";
        _viewModel.TableId = "1";
        _viewModel.NumberOfGuests = "2";

        // Act & Assert
        _viewModel.CreateReservationCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CreateReservationCommand_ShouldBeExecutable_WhenDataValid()
    {
        // Arrange
        _viewModel.CustomerName = "John Doe";
        _viewModel.CustomerPhone = "123456789";
        _viewModel.CustomerEmail = "john@example.com";
        _viewModel.TableId = "1";
        _viewModel.NumberOfGuests = "2";

        // Act & Assert
        _viewModel.CreateReservationCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldCreateReservation()
    {
        // Arrange
        var date = DateTime.Now;
        var expectedReservation = new Reservation(
            1,
            "John Doe",
            "123456789",
            "john@example.com",
            date,
            date.AddHours(18),
            1,
            4,
            "confirmed",
            null
        );
        
        _viewModel.CustomerName = "John Doe";
        _viewModel.CustomerPhone = "123456789";
        _viewModel.CustomerEmail = "john@example.com";
        _viewModel.ReservationDate = date;
        _viewModel.ReservationTime = date.AddHours(18);
        _viewModel.TableId = "1";
        _viewModel.NumberOfGuests = "4";
        _viewModel.SpecialRequests = "";
        
        _useCaseMock.Setup(u => u.CreateReservationAsync(
                "John Doe",
                "123456789",
                "john@example.com",
                date,
                date.AddHours(18),
                1,
                4,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReservation);
        
        _useCaseMock.Setup(u => u.GetReservationsForDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Reservation> { expectedReservation });

        // Act
        await _viewModel.CreateReservationCommand.ExecuteAsync();

        // Assert
        _viewModel.Reservations.Should().HaveCount(1);
        _viewModel.CustomerName.Should().Be("");
        _viewModel.TableId.Should().Be("");
        _useCaseMock.Verify(u => u.CreateReservationAsync(
            "John Doe",
            "123456789",
            "john@example.com",
            date,
            date.AddHours(18),
            1,
            4,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldCancelReservation()
    {
        // Arrange
        _viewModel.CancelReservationId = "1";
        _useCaseMock.Setup(u => u.CancelReservationAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _useCaseMock.Setup(u => u.GetReservationsForDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Reservation>());

        // Act
        await _viewModel.CancelReservationCommand.ExecuteAsync();

        // Assert
        _viewModel.CancelReservationId.Should().Be("");
        _viewModel.Reservations.Should().BeEmpty();
        _useCaseMock.Verify(u => u.CancelReservationAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
