namespace CertMonitor.Agent.Models;

public sealed class CertificateInfoDto
{
    public string Thumbprint { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Subject { get; init; } = string.Empty;

    public string SerialNumber { get; init; } = string.Empty;

    public DateTime ValidFromUtc { get; init; }

    public DateTime ValidToUtc { get; init; }

    public string Algorithm { get; init; } = string.Empty;

    public string SourceType { get; init; } = "Rutoken";

    public string TokenSerialNumber { get; init; } = string.Empty;
}
