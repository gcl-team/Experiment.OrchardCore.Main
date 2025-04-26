using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class EndpointStatisticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EndpointStatisticsMiddleware> _logger;
    private readonly IAmazonCloudWatch _cloudWatch;

    public EndpointStatisticsMiddleware(RequestDelegate next, ILogger<EndpointStatisticsMiddleware> logger, IAmazonCloudWatch cloudWatch)
    {
        _next = next;
        _logger = logger;
        _cloudWatch = cloudWatch;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();

        // Call the next middleware in the pipeline
        await _next(httpContext);

        stopwatch.Stop();

        string endpointPath = httpContext.Request.Path.ToString();

        var responseTimeMs = stopwatch.ElapsedMilliseconds;

        PutResponseTimeEmfData(endpointPath, responseTimeMs);

        // Track the response status code
        if (httpContext.Response.StatusCode / 100 == 2)         await PutCustomMetricData(endpointPath, "Http2xxCount", 1);
        else if (httpContext.Response.StatusCode / 100 == 3)    await PutCustomMetricData(endpointPath, "Http3xxCount", 1);
        else if (httpContext.Response.StatusCode / 100 == 4)    await PutCustomMetricData(endpointPath, "Http4xxCount", 1);
        else if (httpContext.Response.StatusCode / 100 == 5)    await PutCustomMetricData(endpointPath, "Http5xxCount", 1);
    }

    private void PutResponseTimeEmfData(string endpointPath, long responseTimeMs)
    {
        // Create the EMF structure for CloudWatch logs
        var emfLog = new
        {
            _aws = new
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                CloudWatchMetrics = new[]
                {
                    new
                    {
                        Namespace = "Experiment.OrchardCore.Main/Performance",
                        Dimensions = new[] { new[] { "Endpoint" } },
                        Metrics = new[]
                        {
                            new { Name = "ResponseTimeMs", Unit = "Milliseconds" }
                        }
                    }
                }
            },
            Endpoint = endpointPath,
            ResponseTimeMs = responseTimeMs
        };

        // Log it in the required format
        var emfJson = JsonSerializer.Serialize(emfLog);
        _logger.LogInformation(emfJson);
    }

    private async Task PutCustomMetricData(string endpointPath, string metricName, double value)
    {
        var metricData = new MetricDatum
        {
            MetricName = metricName,
            Value = value,
            Unit = StandardUnit.Count,
            Dimensions = new List<Dimension>
            {
                new Dimension
                {
                    Name = "Endpoint", 
                    Value = endpointPath
                }
            }
        };

        var request = new PutMetricDataRequest
        {
            Namespace = "Experiment.OrchardCore.Main/Performance",
            MetricData = new List<MetricDatum> { metricData }
        };

        await _cloudWatch.PutMetricDataAsync(request);
    }
}
