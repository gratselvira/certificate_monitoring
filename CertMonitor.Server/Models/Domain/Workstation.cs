namespace CertMonitor.Server.Models.Domain;

public sealed class Workstation
{
    public int Id { get; set; }

    public string DeviceUid { get; set; } = string.Empty;

    public string Hostname { get; set; } = string.Empty;

    public DateTime FirstSeenAtUtc { get; set; }

    public DateTime LastSeenAtUtc { get; set; }

    public Agent? Agent { get; set; }

    public ICollection<TokenDevice> TokenDevices { get; set; } = new List<TokenDevice>();

    public ICollection<ScanSession> ScanSessions { get; set; } = new List<ScanSession>();
}
