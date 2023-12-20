namespace RobotMonitor.Models.Robot;

public record RobotConfig
{
    public required string IpAddress { get; init; }
    public required int Port { get; init; }
    public required int CameraPort { get; init; }
}