namespace RestAll.Desktop.Core.Reservations;

public sealed record Reservation(
    int Id,
    string CustomerName,
    string CustomerPhone,
    string CustomerEmail,
    DateTime ReservationDate,
    DateTime ReservationTime,
    int TableId,
    int NumberOfGuests,
    string Status,
    string? SpecialRequests
);
