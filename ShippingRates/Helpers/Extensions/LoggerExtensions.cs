using Microsoft.Extensions.Logging;

namespace ShippingRates.Helpers.Extensions
{
    internal static class LoggerExtensions
    {
        internal static void TraceJson<T>(this ILogger<T> logger, string message, object obj)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("{message}: {obj}", message, System.Text.Json.JsonSerializer.Serialize(obj));
            }
        }
    }
}
