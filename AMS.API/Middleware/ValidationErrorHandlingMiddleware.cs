using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace AMS.API.Middleware
{
    public class ValidationErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationErrorHandlingMiddleware> _logger;

        public ValidationErrorHandlingMiddleware(RequestDelegate next, ILogger<ValidationErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Handle validation error responses (400 Bad Request)
                if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    _logger.LogWarning("Validation error for request: {Path}", context.Request.Path);
                }
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ex.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList())
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
