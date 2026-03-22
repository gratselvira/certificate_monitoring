namespace CertMonitor.Server.ViewModels;

public sealed class DashboardViewModel
{
    public string? LatestHostname { get; init; }

    public DateTime? LastScanAtUtc { get; init; }

    public int TokensCount { get; init; }

    public int CertificatesCount { get; init; }

    public int ScanSessionsCount { get; init; }
}
