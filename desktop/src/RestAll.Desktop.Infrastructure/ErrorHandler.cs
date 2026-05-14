using Microsoft.Extensions.Logging;
using System.Text.Json;
using RestAll.Desktop.Core.Exceptions;

namespace RestAll.Desktop.Infrastructure;

public class ErrorHandler : IErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(ILogger<ErrorHandler> logger)
    {
        _logger = logger;
    }

    public RestAllException HandleException(Exception exception, string? context = null)
    {
        var errorType = exception.GetType().Name;
        var errorCode = $"{errorType}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        
        var userMessage = GetUserFriendlyMessage(exception, context);
        
        _logger.LogError(exception, "Error {ErrorCode} in {Context}: {Message}", errorCode, context ?? "Unknown", exception.Message);
        
        return new RestAllException(errorCode, userMessage, exception.Message, exception);
    }

    public string GetUserFriendlyMessage(Exception exception, string? context = null)
    {
        return exception switch
        {
            HttpRequestException => context switch
            {
                "orders" => "Nie można połączyć się z serwerem zamówień. Sprawdź połączenie z internetem.",
                "menu" => "Nie można pobrać menu. Sprawdź połączenie z internetem.",
                "auth" => "Problem z logowaniem. Spróbuj ponownie później.",
                "profile" => "Nie można zaktualizować profil. Sprawdź połączenie z internetem.",
                "reservations" => "Nie można zarządzać rezerwacjami. Sprawdź połączenie z internetem.",
                "kitchen" => "Nie można połączyć się z kuchnią. Sprawdź połączenie z internetem.",
                "tables" => "Nie można zarządzać stolikami. Sprawdź połączenie z internetem.",
                _ => "Problem z połączeniem z serwerem. Sprawdź połączenie z internetem."
            },
            TaskCanceledException => "Operacja została przerwana. Spróbuj ponownie.",
            TimeoutException => "Przekroczono czas oczekiwania. Spróbuj ponownie.",
            UnauthorizedAccessException => "Brak uprawnień. Zaloguj się ponownie.",
            JsonException => "Problem z przetwarzaniem danych. Skontaktuj się z administratorem.",
            _ => "Wystąpił nieoczekiwany błąd. Spróbuj ponownie później."
        };
    }
}
