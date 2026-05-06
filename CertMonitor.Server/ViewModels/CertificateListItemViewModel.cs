namespace CertMonitor.Server.ViewModels;

public sealed class CertificateListItemViewModel
{
    public string Subject { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string SerialNumber { get; init; } = string.Empty;

    public string? Thumbprint { get; init; }

    public DateTime ValidFromUtc { get; init; }

    public DateTime ValidToUtc { get; init; }

    public string Algorithm { get; init; } = string.Empty;

    public string? TokenSerialNumber { get; init; }

    public DateTime LastSeenAtUtc { get; init; }
}
