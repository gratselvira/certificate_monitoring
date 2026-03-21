namespace CertMonitor.Agent.Models;

public sealed class WorkstationInfo
{
    public string DeviceUid { get; init; } = string.Empty;

    public string Hostname { get; init; } = string.Empty;
}
