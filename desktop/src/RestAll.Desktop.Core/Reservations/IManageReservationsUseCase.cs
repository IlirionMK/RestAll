namespace RestAll.Desktop.Core.Reservations;

public interface IManageReservationsUseCase
{
    Task<List<Reservation>> GetReservationsForDateAsync(DateTime date, CancellationToken cancellationToken);
    Task<Reservation?> CreateReservationAsync(string customerName, string customerPhone, string customerEmail, DateTime reservationDate, DateTime reservationTime, int tableId, int numberOfGuests, string? specialRequests, CancellationToken cancellationToken);
    Task<bool> CancelReservationAsync(int id, CancellationToken cancellationToken);
}
