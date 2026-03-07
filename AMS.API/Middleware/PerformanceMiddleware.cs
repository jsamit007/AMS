using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AMS.API.Middleware
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;
        private const long SlowRequestThresholdMs = 1000; // 1 second

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Store start time in HttpContext items for potential use in other middleware/controllers
                context.Items["RequestStartTime"] = DateTime.UtcNow;
                
                await _next(context);
                
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                
                // Log slow requests
                if (elapsedMs > SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                        context.Request.Method,
                        context.Request.Path,
                        elapsedMs,
                        context.Response.StatusCode);
                }
                else
                {
                    _logger.LogInformation(
                        "Request completed: {Method} {Path} in {ElapsedMs}ms with status {StatusCode}",
                        context.Request.Method,
                        context.Request.Path,
                        elapsedMs,
                        context.Response.StatusCode);
                }
                
                // Add performance headers to response
                context.Response.Headers["X-Response-Time-Ms"] = elapsedMs.ToString();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    "Request failed: {Method} {Path} after {ElapsedMs}ms - {Exception}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
    }
}
