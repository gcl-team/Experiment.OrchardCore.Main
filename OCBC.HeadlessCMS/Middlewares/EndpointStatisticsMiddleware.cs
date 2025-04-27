using System.Diagnostics;
using System.Text.Json;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using OCBC.HeadlessCMS.BackgroundServices;

public class EndpointStatisticsMiddleware(
    RequestDelegate next, 
    ILogger<EndpointStatisticsMiddleware> logger, 
    IAmazonCloudWatch cloudWatch, 
    MetricsPublisher metricsPublisher)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();

        // Call the next middleware in the pipeline
        await next(httpContext);

        stopwatch.Stop();

        string endpointPath = httpContext.Request.Path.ToString();

        var responseTimeMs = stopwatch.ElapsedMilliseconds;

        PutResponseTimeEmfData(endpointPath, responseTimeMs);

        // Track the response status code
        // if (httpContext.Response.StatusCode / 100 == 2)         await PutCustomMetricDataAsync(endpointPath, "Http2xxCount", 1);
        // else if (httpContext.Response.StatusCode / 100 == 3)    await PutCustomMetricDataAsync(endpointPath, "Http3xxCount", 1);
        // else if (httpContext.Response.StatusCode / 100 == 4)    await PutCustomMetricDataAsync(endpointPath, "Http4xxCount", 1);
        // else if (httpContext.Response.StatusCode / 100 == 5)    await PutCustomMetricDataAsync(endpointPath, "Http5xxCount", 1);
        
        switch (httpContext.Response.StatusCode / 100)
        {
            case 2:
                PutCustomMetricDataWithMetricsPublisher(endpointPath, "Http2xxCount", 1);
                break;
            case 3:
                PutCustomMetricDataWithMetricsPublisher(endpointPath, "Http3xxCount", 1);
                break;
            case 4:
                PutCustomMetricDataWithMetricsPublisher(endpointPath, "Http4xxCount", 1);
                break;
            case 5:
                PutCustomMetricDataWithMetricsPublisher(endpointPath, "Http5xxCount", 1);
                break;
        }
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
        logger.LogInformation(emfJson);
    }

    private async Task PutCustomMetricDataAsync(string endpointPath, string metricName, double value)
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

        await cloudWatch.PutMetricDataAsync(request);
    }
    
    private void PutCustomMetricDataWithMetricsPublisher(string endpointPath, string metricName, double value)
    {
        metricsPublisher.TrackMetric(metricName, value, endpointPath);
    }
}
