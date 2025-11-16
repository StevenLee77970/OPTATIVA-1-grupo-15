namespace PlasticHouseETL.Infrastructure.Configuration;

public class ETLSettings
{
    public int BatchSize { get; set; } = 1000;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public int ExecutionIntervalHours { get; set; } = 6;
    public DateTime? LastExecutionDate { get; set; }
}

