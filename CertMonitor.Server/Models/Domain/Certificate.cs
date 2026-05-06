namespace CertMonitor.Server.Models.Domain;

public sealed class Certificate
{
    public int Id { get; set; }

    public string? Thumbprint { get; set; }

    public string Issuer { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public DateTime ValidFromUtc { get; set; }

    public DateTime ValidToUtc { get; set; }

    public string Algorithm { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public int? TokenDeviceId { get; set; }

    public DateTime FirstSeenAtUtc { get; set; }

    public DateTime LastSeenAtUtc { get; set; }

    public TokenDevice? TokenDevice { get; set; }

    public ICollection<CertificateDetection> CertificateDetections { get; set; } = new List<CertificateDetection>();
}
