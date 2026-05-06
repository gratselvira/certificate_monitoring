namespace CertMonitor.Server.Models.Domain;

public sealed class CertificateDetection
{
    public int Id { get; set; }

    public int ScanSessionId { get; set; }

    public int CertificateId { get; set; }

    public int? TokenDeviceId { get; set; }

    public DateTime DetectedAtUtc { get; set; }

    public ScanSession ScanSession { get; set; } = null!;

    public Certificate Certificate { get; set; } = null!;

    public TokenDevice? TokenDevice { get; set; }
}
