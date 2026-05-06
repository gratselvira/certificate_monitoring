namespace CertMonitor.Server.Models.Domain;

public sealed class ScanSession
{
    public int Id { get; set; }

    public string SessionUid { get; set; } = string.Empty;

    public int WorkstationId { get; set; }

    public int AgentId { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime FinishedAtUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TokensFoundCount { get; set; }

    public int CertificatesFoundCount { get; set; }

    public string RawPayloadJson { get; set; } = string.Empty;

    public Workstation Workstation { get; set; } = null!;

    public Agent Agent { get; set; } = null!;

    public ICollection<CertificateDetection> CertificateDetections { get; set; } = new List<CertificateDetection>();
}
