namespace CertMonitor.Agent.Models;

public sealed class TokenInfoDto
{
    public string SerialNumber { get; init; } = string.Empty;

    public string TokenType { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string Manufacturer { get; init; } = string.Empty;

    public string Pkcs11SlotId { get; init; } = string.Empty;
}
