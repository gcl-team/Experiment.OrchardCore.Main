namespace OCBC.HeadlessCMS.Models;

public class MetricsOptions
{
    public int FlushIntervalSeconds { get; set; } = 60;
    public string Namespace { get; set; } = "Experiment.OrchardCore.Main/Performance";
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}
