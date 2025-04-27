using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using OCBC.HeadlessCMS.Models;

namespace OCBC.HeadlessCMS.BackgroundServices;

public class MetricsPublisher : BackgroundService
{
    private readonly IAmazonCloudWatch _cloudWatch;
    private readonly MetricsOptions _options;
    private readonly ConcurrentBag<MetricDatum> _pendingMetrics = new();
    private readonly ILogger<MetricsPublisher> _logger;

    public MetricsPublisher(IAmazonCloudWatch cloudWatch, IOptions<MetricsOptions> options, ILogger<MetricsPublisher> logger)
    {
        _cloudWatch = cloudWatch;
        _options = options.Value;
        _logger = logger;
    }

    public void TrackMetric(string metricName, double value, string endpointPath)
    {
        _pendingMetrics.Add(new MetricDatum
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
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MetricsPublisher started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.FlushIntervalSeconds), stoppingToken);
            await FlushMetricsAsync();
        }
    }

    private async Task FlushMetricsAsync()
    {
        if (_pendingMetrics.IsEmpty) return;

        // The array MetricData can include no more than 1000 metrics per call.
        //
        //  Reference: https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/API_PutMetricData.html#API_PutMetricData_RequestParameters
        const int MaxMetricsPerRequest = 1000;

        var metricsToSend = new List<MetricDatum>();
        var metricsCount = 0;
        while (_pendingMetrics.TryTake(out var datum))
        {
            metricsToSend.Add(datum);

            metricsCount += 1;
            if (metricsCount >= MaxMetricsPerRequest) break;
        }

        var request = new PutMetricDataRequest
        {
            Namespace = _options.Namespace,
            MetricData = metricsToSend
        };

        int attempt = 0;
        while (attempt < _options.MaxRetryAttempts)
        {
            try
            {
                await _cloudWatch.PutMetricDataAsync(request);
                _logger.LogInformation("Flushed {Count} metrics to CloudWatch.", metricsToSend.Count);
                break;
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogWarning(ex, "Failed to flush metrics. Attempt {Attempt}/{MaxAttempts}", attempt, _options.MaxRetryAttempts);
                if (attempt < _options.MaxRetryAttempts)
                    await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds));
                else
                    _logger.LogError("Max retry attempts reached. Dropping {Count} metrics.", metricsToSend.Count);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MetricsPublisher stopping.");
        await FlushMetricsAsync();
        await base.StopAsync(cancellationToken);
    }
}
