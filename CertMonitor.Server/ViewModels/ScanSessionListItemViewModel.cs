namespace CertMonitor.Server.ViewModels;

public sealed class ScanSessionListItemViewModel
{
    public string SessionUid { get; init; } = string.Empty;

    public DateTime StartedAtUtc { get; init; }

    public DateTime FinishedAtUtc { get; init; }

    public string Status { get; init; } = string.Empty;

    public int TokensFoundCount { get; init; }

    public int CertificatesFoundCount { get; init; }

    public string WorkstationHostname { get; init; } = string.Empty;

    public string StatusDisplay =>
        Status switch
        {
            "Processed" => "Обработано",
            _ => Status
        };
}
