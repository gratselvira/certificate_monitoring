namespace CertMonitor.Agent.Models;

public sealed class ScanResult
{
    public WorkstationInfo Workstation { get; init; } = new();

    public AgentInfo Agent { get; init; } = new();

    public ScanSessionInfo ScanSession { get; init; } = new();

    public List<TokenInfoDto> Tokens { get; init; } = new();

    public List<CertificateInfoDto> Certificates { get; init; } = new();
}
