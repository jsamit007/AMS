using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AMS.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly Dictionary<string, List<DateTime>> RequestLog = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, int maxRequests = 100, int timeWindowSeconds = 60)
        {
            _next = next;
            _logger = logger;
            _maxRequests = maxRequests;
            _timeWindow = TimeSpan.FromSeconds(timeWindowSeconds);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;
            bool isRateLimited = false;

            lock (RequestLog)
            {
                if (!RequestLog.ContainsKey(clientId))
                {
                    RequestLog[clientId] = new List<DateTime>();
                }

                // Remove old requests outside the time window
                RequestLog[clientId] = RequestLog[clientId].Where(d => now - d < _timeWindow).ToList();

                // Check if rate limit exceeded
                if (RequestLog[clientId].Count >= _maxRequests)
                {
                    isRateLimited = true;
                }
                else
                {
                    // Log the request
                    RequestLog[clientId].Add(now);
                }
            }

            if (isRateLimited)
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Rate limit exceeded. Please try again later.",
                    errors = new[] { "Too many requests" }
                });
                return;
            }

            await _next(context);
        }
    }
}
