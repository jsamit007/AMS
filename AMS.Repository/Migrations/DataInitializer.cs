using System.Reflection;
using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMS.Repository.Migrations
{
    /// <summary>
    /// Handles database initialization and migration execution using DbUp for PostgreSQL
    /// </summary>
    public static class DataInitializer
    {
        /// <summary>
        /// Initializes the database by running all pending migrations
        /// </summary>
        /// <param name="services">Service provider containing database configuration</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task InitializeDatabaseAsync(IServiceProvider services)
        {
            try
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
                }

                var upgrader = DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    throw new InvalidOperationException("Database migration failed: " + result.Error.Message, result.Error);
                }

                Console.WriteLine("Database migration completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the journal table name used by DbUp to track migrations
        /// </summary>
        /// <returns>Journal table name</returns>
        public static string GetJournalTableName()
        {
            return "schemaversions";
        }
    }
}
