namespace CertMonitor.Server.Models.Dto;

public sealed class ScanResultResponse
{
    public bool Success { get; init; }

    public int? ScanSessionId { get; init; }

    public int TokensSaved { get; init; }

    public int CertificatesSaved { get; init; }

    public string Message { get; init; } = string.Empty;
}
