namespace CertMonitor.Server.Models.Domain;

public sealed class TokenDevice
{
    public int Id { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public string TokenType { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string Pkcs11SlotId { get; set; } = string.Empty;

    public int WorkstationId { get; set; }

    public DateTime FirstSeenAtUtc { get; set; }

    public DateTime LastSeenAtUtc { get; set; }

    public Workstation Workstation { get; set; } = null!;

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public ICollection<CertificateDetection> CertificateDetections { get; set; } = new List<CertificateDetection>();
}
