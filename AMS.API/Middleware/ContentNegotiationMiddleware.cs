using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace AMS.API.Middleware
{
    public class ContentNegotiationMiddleware
    {
        public static void ConfigureContentNegotiation(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                // Add support for multiple content types
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
            })
            .AddXmlSerializerFormatters(); // Support XML responses if needed
        }
    }
}
