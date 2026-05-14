using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Reservations;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Reservations;

public sealed class HttpReservationGateway : IReservationGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly ILogger<HttpReservationGateway> _logger;

    public HttpReservationGateway(HttpClient httpClient, RestAllApiOptions options, ILogger<HttpReservationGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<List<Reservation>> GetReservationsAsync(DateTime date, CancellationToken cancellationToken)
    {
        try
        {
            var dateString = date.ToString("yyyy-MM-dd");
            _logger.LogInformation("Fetching reservations from {Endpoint} for date {Date}", 
                $"{_options.BaseUrl}/reservations?date={dateString}", dateString);
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/reservations?date={dateString}", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Reservations response: HTTP {StatusCode}, Content: {ContentLength} bytes", 
                response.StatusCode, responseContent.Length);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch reservations: {Response}", responseContent);
                return new List<Reservation>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Array)
            {
                return new List<Reservation>();
            }

            var reservations = new List<Reservation>();
            foreach (var element in data.EnumerateArray())
            {
                var reservation = ParseReservation(element);
                if (reservation is not null)
                {
                    reservations.Add(reservation);
                }
            }

            return reservations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reservations for date {Date}", date);
            return new List<Reservation>();
        }
    }

    public async Task<Reservation?> CreateReservationAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                customer_name = reservation.CustomerName,
                customer_phone = reservation.CustomerPhone,
                customer_email = reservation.CustomerEmail,
                reservation_date = reservation.ReservationDate.ToString("yyyy-MM-dd"),
                reservation_time = reservation.ReservationTime.ToString("HH:mm"),
                table_id = reservation.TableId,
                number_of_guests = reservation.NumberOfGuests,
                special_requests = reservation.SpecialRequests
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/reservations", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ParseReservation(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation");
            return null;
        }
    }

    public async Task<bool> CancelReservationAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_options.BaseUrl}/reservations/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
            return false;
        }
    }

    private Reservation? ParseReservation(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetStringProperty(element, "customer_name", out var customerName) ||
            !JsonParserHelper.TryGetStringProperty(element, "customer_phone", out var customerPhone) ||
            !JsonParserHelper.TryGetStringProperty(element, "customer_email", out var customerEmail) ||
            !TryGetDateTime(element, "reservation_date", out var reservationDate) ||
            !TryGetDateTime(element, "reservation_time", out var reservationTime) ||
            !JsonParserHelper.TryGetIntProperty(element, "table_id", out var tableId) ||
            !JsonParserHelper.TryGetIntProperty(element, "number_of_guests", out var numberOfGuests) ||
            !JsonParserHelper.TryGetStringProperty(element, "status", out var status))
        {
            return null;
        }

        var specialRequests = JsonParserHelper.TryGetStringProperty(element, "special_requests", out var specialReqs) ? specialReqs : null;

        return new Reservation(
            id,
            customerName ?? string.Empty,
            customerPhone ?? string.Empty,
            customerEmail ?? string.Empty,
            reservationDate,
            reservationTime,
            tableId,
            numberOfGuests,
            status ?? string.Empty,
            specialRequests
        );
    }

    private static bool TryGetDateTime(JsonElement element, string propertyName, out DateTime dateTime)
    {
        dateTime = default;

        if (JsonParserHelper.TryGetStringProperty(element, propertyName, out var dateString))
        {
            if (DateTime.TryParse(dateString, out var parsed))
            {
                dateTime = parsed;
                return true;
            }
        }

        if (element.TryGetProperty(propertyName, out var dateElement))
        {
            if (dateElement.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(dateElement.GetString(), out var parsed))
                {
                    dateTime = parsed;
                    return true;
                }
            }
        }

        return false;
    }
}
