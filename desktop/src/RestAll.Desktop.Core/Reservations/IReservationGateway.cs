namespace RestAll.Desktop.Core.Reservations;

public interface IReservationGateway
{
    Task<List<Reservation>> GetReservationsAsync(DateTime date, CancellationToken cancellationToken);
    Task<Reservation?> CreateReservationAsync(Reservation reservation, CancellationToken cancellationToken);
    Task<bool> CancelReservationAsync(int id, CancellationToken cancellationToken);
}
