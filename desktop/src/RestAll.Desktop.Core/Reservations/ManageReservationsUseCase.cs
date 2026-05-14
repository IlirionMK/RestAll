namespace RestAll.Desktop.Core.Reservations;

public sealed class ManageReservationsUseCase : IManageReservationsUseCase
{
    private readonly IReservationGateway _gateway;

    public ManageReservationsUseCase(IReservationGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<List<Reservation>> GetReservationsForDateAsync(DateTime date, CancellationToken cancellationToken)
    {
        return await _gateway.GetReservationsAsync(date, cancellationToken);
    }

    public async Task<Reservation?> CreateReservationAsync(string customerName, string customerPhone, string customerEmail, DateTime reservationDate, DateTime reservationTime, int tableId, int numberOfGuests, string? specialRequests, CancellationToken cancellationToken)
    {
        var reservation = new Reservation(
            0,
            customerName,
            customerPhone,
            customerEmail,
            reservationDate,
            reservationTime,
            tableId,
            numberOfGuests,
            "confirmed",
            specialRequests
        );

        return await _gateway.CreateReservationAsync(reservation, cancellationToken);
    }

    public async Task<bool> CancelReservationAsync(int id, CancellationToken cancellationToken)
    {
        return await _gateway.CancelReservationAsync(id, cancellationToken);
    }
}
