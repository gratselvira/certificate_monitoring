namespace CertMonitor.Agent.Configuration;

public sealed class AgentOptions
{
    public bool UseMockScanner { get; init; } = true;

    public string? Pkcs11LibraryPath { get; init; }

    public bool OutputJsonEnabled { get; init; } = true;

    public string OutputJsonPath { get; init; } = "scan-result.json";
}
