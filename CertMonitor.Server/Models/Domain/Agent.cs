namespace CertMonitor.Server.Models.Domain;

public sealed class Agent
{
    public int Id { get; set; }

    public int WorkstationId { get; set; }

    public string AgentVersion { get; set; } = string.Empty;

    public DateTime LastScanAtUtc { get; set; }

    public Workstation Workstation { get; set; } = null!;

    public ICollection<ScanSession> ScanSessions { get; set; } = new List<ScanSession>();
}
