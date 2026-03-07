using AMS.Repository.Context;
using AMS.Repository.Interfaces;
using AMS.Repository.Repository;
using AMS.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AMS.Repository.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(
            this IServiceCollection services,
            string connectionString)
        {
            // Add DbContext for PostgreSQL
            services.AddDbContext<AttendanceManagementContext>(options =>
                options.UseNpgsql(connectionString));

            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            // Add Repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<ILeaveRepository, LeaveRepository>();

            return services;
        }

        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AttendanceManagementContext>();
            
            // Apply migrations and create database if it doesn't exist
            await context.Database.MigrateAsync();
        }
    }
}
