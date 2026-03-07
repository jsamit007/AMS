using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AMS.Repository.Extensions;
using AMS.Contracts.Behaviors;
using AMS.Contracts.Validators;
using AMS.Contracts.DTOs;
using FluentValidation;
using MediatR;

namespace AMS.API.Middleware
{
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds all required services for the API
        /// </summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get database connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Add repositories
            services.AddRepositories(connectionString);

            // Add FluentValidation - manually register validators
            services.AddScoped<IValidator<CreateAttendanceDto>, CreateAttendanceDtoValidator>();
            services.AddScoped<IValidator<UpdateAttendanceDto>, UpdateAttendanceDtoValidator>();
            services.AddScoped<IValidator<CreateEmployeeDto>, CreateEmployeeDtoValidator>();
            services.AddScoped<IValidator<UpdateEmployeeDto>, UpdateEmployeeDtoValidator>();
            services.AddScoped<IValidator<CreateDepartmentDto>, CreateDepartmentDtoValidator>();
            services.AddScoped<IValidator<UpdateDepartmentDto>, UpdateDepartmentDtoValidator>();
            services.AddScoped<IValidator<CreateLeaveDto>, CreateLeaveDtoValidator>();
            services.AddScoped<IValidator<UpdateLeaveDto>, UpdateLeaveDtoValidator>();

            // Add MediatR for CQRS - scan both Command and Query assemblies
            services.AddMediatR(cfg =>
            {
                // Scan for handlers in Command and Query projects
                var assembly1 = typeof(AMS.Command.Commands.Attendance.MarkAttendanceCommand).Assembly;
                var assembly2 = typeof(AMS.Query.Queries.Attendance.GetAttendanceQuery).Assembly;
                cfg.RegisterServicesFromAssemblies(assembly1, assembly2);
                
                // Register validation behavior for automatic validation
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // Configure CORS
            CorsPolicyMiddleware.ConfigureCors(services, configuration);

            // Configure Authentication
            AuthenticationMiddleware.ConfigureAuthentication(services, configuration);

            // Configure Authorization
            AuthorizationMiddleware.ConfigureAuthorization(services);

            // Configure Content Negotiation
            ContentNegotiationMiddleware.ConfigureContentNegotiation(services);

            // Configure CloudWatch Logging
            CloudWatchLoggingConfiguration.ConfigureCloudWatchLogging(services, configuration);

            // Add logging
            services.AddLogging(config =>
            {
                config.ClearProviders();
                config.AddConsole();
                config.AddDebug();
                config.SetMinimumLevel(LogLevel.Information);
                
                // Configure CloudWatch logging provider
                CloudWatchLoggingConfiguration.ConfigureCloudWatchLogging(config, configuration);
            });

            // Add API versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            return services;
        }

        /// <summary>
        /// Adds all required middlewares to the HTTP request pipeline
        /// </summary>
        public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
        {
            // Exception handling should be first
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Performance monitoring - measure response times
            app.UseMiddleware<PerformanceMiddleware>();

            // Request/Response logging
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            // Rate limiting
            app.UseMiddleware<RateLimitingMiddleware>();

            // HTTPS redirection
            app.UseHttpsRedirection();

            // CORS - must be before Authorization
            app.UseCors(CorsPolicyMiddleware.CorsPolicy);

            // Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Validation error handling
            app.UseMiddleware<ValidationErrorHandlingMiddleware>();

            return app;
        }
    }
}
