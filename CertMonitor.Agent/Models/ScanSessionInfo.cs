namespace CertMonitor.Agent.Models;

public sealed class ScanSessionInfo
{
    public string SessionUid { get; init; } = string.Empty;

    public DateTime StartedAtUtc { get; init; }

    public DateTime FinishedAtUtc { get; init; }
}
