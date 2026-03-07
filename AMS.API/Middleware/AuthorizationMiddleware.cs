using Microsoft.Extensions.DependencyInjection;

namespace AMS.API.Middleware
{
    public class AuthorizationMiddleware
    {
        public static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
                .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager", "Admin"))
                .AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee", "Manager", "Admin"))
                .AddPolicy("ReportAccess", policy => policy.RequireRole("Manager", "Admin", "HR"))
                .AddPolicy("LeaveApproval", policy => policy.RequireRole("Manager", "Admin", "HR"));
        }
    }
}
