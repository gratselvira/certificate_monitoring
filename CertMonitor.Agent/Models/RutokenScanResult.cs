namespace CertMonitor.Agent.Models;

public sealed class RutokenScanResult
{
    public bool LibraryAvailable { get; init; } = true;

    public bool RutokenFound { get; init; }

    public List<TokenInfoDto> Tokens { get; init; } = new();

    public List<CertificateInfoDto> Certificates { get; init; } = new();

    public List<string> Messages { get; init; } = new();
}
