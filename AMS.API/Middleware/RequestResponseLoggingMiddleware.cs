using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AMS.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log request
            _logger.LogInformation("HTTP Request: {Method} {Path}", context.Request.Method, context.Request.Path);

            // Store original response body stream
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                var startTime = DateTime.UtcNow;
                await _next(context);
                var duration = DateTime.UtcNow - startTime;

                // Log response
                _logger.LogInformation("HTTP Response: {StatusCode} {Method} {Path} - {Duration}ms",
                    context.Response.StatusCode, context.Request.Method, context.Request.Path, duration.TotalMilliseconds);

                // Copy response body to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
