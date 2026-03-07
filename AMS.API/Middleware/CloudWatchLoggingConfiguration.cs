using Amazon.CloudWatchLogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace AMS.API.Middleware
{
    public class CloudWatchLoggingConfiguration
    {
        public static void ConfigureCloudWatchLogging(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var cloudWatchSettings = configuration.GetSection("CloudWatch");
            var isEnabled = cloudWatchSettings.GetValue<bool>("Enabled");

            if (!isEnabled)
            {
                return;
            }

            var logGroupName = cloudWatchSettings.GetValue<string>("LogGroupName") ?? "ams-api";
            var awsRegion = cloudWatchSettings.GetValue<string>("Region") ?? "us-east-1";
            var accessKey = cloudWatchSettings.GetValue<string>("AccessKey");
            var secretKey = cloudWatchSettings.GetValue<string>("SecretKey");

            try
            {
                // Add AWS CloudWatch client
                var cloudWatchClient = string.IsNullOrEmpty(accessKey)
                    ? new AmazonCloudWatchLogsClient(Amazon.RegionEndpoint.GetBySystemName(awsRegion))
                    : new AmazonCloudWatchLogsClient(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(awsRegion));

                services.AddSingleton<IAmazonCloudWatchLogs>(cloudWatchClient);

                // Configure Serilog with file and console sinks
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File(
                        "logs/ams-api-.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to configure CloudWatch logging: {ex.Message}");
                // Gracefully fallback to standard logging if CloudWatch setup fails
            }
        }

        public static void ConfigureCloudWatchLogging(ILoggingBuilder loggingBuilder, IConfiguration configuration)
        {
            var cloudWatchSettings = configuration.GetSection("CloudWatch");
            var isEnabled = cloudWatchSettings.GetValue<bool>("Enabled");

            if (!isEnabled)
            {
                return;
            }

            try
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to add CloudWatch logging provider: {ex.Message}");
            }
        }
    }
}
