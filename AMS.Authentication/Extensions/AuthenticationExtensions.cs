using AMS.Authentication.Configuration;
using AMS.Authentication.Services;
using AMS.Authentication.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AMS.Authentication.Extensions
{
    /// <summary>
    /// Extension methods for registering authentication services
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Add authentication services to the dependency injection container
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure authentication settings
            services.Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));
            var authSettings = configuration.GetSection("Authentication").Get<AuthenticationSettings>()
                ?? throw new InvalidOperationException("Authentication configuration not found");

            // Register authentication services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRoleService, RoleService>();

            // Configure JWT authentication
            var jwtSettings = authSettings.Jwt;
            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds),
                    RequireExpirationTime = true
                };

                // Event handlers for JWT bearer authentication
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Custom token validation logic
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Add authorization policies for fine-grained access control
        /// </summary>
        public static IServiceCollection AddAuthorizationPolicies(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Admin policy - requires Admin role
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                // Manager policy - requires Manager or Admin role
                options.AddPolicy("ManagerOrAdmin", policy =>
                    policy.RequireRole("Manager", "Admin"));

                // Employee policy - requires valid user
                options.AddPolicy("EmployeeAccess", policy =>
                    policy.RequireAuthenticatedUser());

                // Attendance permission policy
                options.AddPolicy("CanViewAttendance", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permissions" && c.Value.Contains("attendance:read"))));

                options.AddPolicy("CanCreateAttendance", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permissions" && c.Value.Contains("attendance:create"))));

                options.AddPolicy("CanEditAttendance", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permissions" && c.Value.Contains("attendance:update"))));

                options.AddPolicy("CanDeleteAttendance", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permissions" && c.Value.Contains("attendance:delete"))));

                // Report permission policy
                options.AddPolicy("CanViewReports", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permissions" && c.Value.Contains("reports:read"))));

                // Leave management policy
                options.AddPolicy("CanApproveLeave", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permissions" && c.Value.Contains("leave:approve"))));
            });

            return services;
        }
    }
}
