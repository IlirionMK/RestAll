namespace RestAll.Desktop.Core.Exceptions;

public interface IErrorHandler
{
    RestAllException HandleException(Exception exception, string? context = null);
    string GetUserFriendlyMessage(Exception exception, string? context = null);
}
