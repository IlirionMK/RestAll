using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using RestAll.Desktop.Core.Exceptions;
using System.Net.Http;

namespace RestAll.Desktop.App;

public static class RetryPolicyHelper
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(string serviceName)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
