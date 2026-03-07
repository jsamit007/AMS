using AMS.API.Middleware;
using AMS.Repository.Migrations;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add API services
builder.Services.AddApiServices(builder.Configuration);

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Attendance Management System API", Version = "v1" });
    
    // Add JWT Bearer token support
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    };
    
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Use API middleware
app.UseApiMiddleware();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map controllers
app.MapControllers();

// Initialize database and run migrations
try
{
    await DataInitializer.InitializeDatabaseAsync(app.Services);
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
    throw;
}

app.Run();
