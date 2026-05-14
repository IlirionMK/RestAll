namespace RestAll.Desktop.Core.Exceptions;

public class RestAllException : Exception
{
    public string ErrorCode { get; }
    public string UserMessage { get; }

    public RestAllException(string errorCode, string userMessage, Exception? innerException = null)
        : base(userMessage, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }

    public RestAllException(string errorCode, string userMessage, string? technicalMessage, Exception? innerException = null)
        : base(technicalMessage ?? userMessage, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }
}
