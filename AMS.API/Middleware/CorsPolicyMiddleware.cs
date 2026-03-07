using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace AMS.API.Middleware
{
    public class CorsPolicyMiddleware
    {
        public const string CorsPolicy = "AllowSpecificOrigins";

        public static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:3000", "http://localhost:5173" };
            // The X-Total-Count header is used to indicate the total number of items available
            // The X-Total-Pages header is used to indicate the total number of pages available
            // allowcredentials is needed to allow cookies and authorization headers to be sent in cross-origin requests
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, builder =>
                {
                    builder
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("X-Total-Count", "X-Total-Pages");
                });
            });
        }
    }
}
