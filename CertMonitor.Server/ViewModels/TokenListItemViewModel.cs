namespace CertMonitor.Server.ViewModels;

public sealed class TokenListItemViewModel
{
    public string SerialNumber { get; init; } = string.Empty;

    public string TokenType { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string Manufacturer { get; init; } = string.Empty;

    public string Pkcs11SlotId { get; init; } = string.Empty;

    public string WorkstationHostname { get; init; } = string.Empty;

    public DateTime LastSeenAtUtc { get; init; }
}
