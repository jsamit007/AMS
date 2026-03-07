using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Microsoft.Extensions.Logging;

namespace AMS.API.Middleware
{
    public class CloudWatchLoggingMiddleware
    {
        private readonly IAmazonCloudWatchLogs _cloudWatchClient;
        private readonly string _logGroupName;
        private readonly string _logStreamName;
        private readonly ILogger<CloudWatchLoggingMiddleware> _logger;

        public CloudWatchLoggingMiddleware(
            ILogger<CloudWatchLoggingMiddleware> logger,
            string logGroupName = "ams-api",
            string? logStreamName = null)
        {
            _logger = logger;
            _cloudWatchClient = new AmazonCloudWatchLogsClient();
            _logGroupName = logGroupName;
            _logStreamName = logStreamName ?? $"{Environment.MachineName}-{DateTime.UtcNow:yyyy-MM-dd}";

            // Initialize log group and stream if they don't exist
            InitializeCloudWatchLogging().Wait();
        }

        private async Task InitializeCloudWatchLogging()
        {
            try
            {
                // Check if log group exists, if not create it
                var logGroups = await _cloudWatchClient.DescribeLogGroupsAsync();
                if (!logGroups.LogGroups.Any(lg => lg.LogGroupName == _logGroupName))
                {
                    await _cloudWatchClient.CreateLogGroupAsync(new CreateLogGroupRequest
                    {
                        LogGroupName = _logGroupName
                    });
                    _logger.LogInformation("Created CloudWatch log group: {LogGroupName}", _logGroupName);
                }

                // Check if log stream exists, if not create it
                var logStreams = await _cloudWatchClient.DescribeLogStreamsAsync(new DescribeLogStreamsRequest
                {
                    LogGroupName = _logGroupName
                });

                if (!logStreams.LogStreams.Any(ls => ls.LogStreamName == _logStreamName))
                {
                    await _cloudWatchClient.CreateLogStreamAsync(new CreateLogStreamRequest
                    {
                        LogGroupName = _logGroupName,
                        LogStreamName = _logStreamName
                    });
                    _logger.LogInformation("Created CloudWatch log stream: {LogStreamName}", _logStreamName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize CloudWatch logging");
            }
        }

        public async Task PutLogsAsync(string logMessage)
        {
            try
            {
                var putLogEventsRequest = new PutLogEventsRequest
                {
                    LogGroupName = _logGroupName,
                    LogStreamName = _logStreamName,
                    LogEvents = new List<InputLogEvent>
                    {
                        new InputLogEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Message = logMessage
                        }
                    }
                };

                await _cloudWatchClient.PutLogEventsAsync(putLogEventsRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to put logs to CloudWatch");
            }
        }
    }
}
