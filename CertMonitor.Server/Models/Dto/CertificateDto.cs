using System.ComponentModel.DataAnnotations;

namespace CertMonitor.Server.Models.Dto;

public sealed class CertificateDto
{
    public string? Thumbprint { get; init; }

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Subject { get; init; } = string.Empty;

    [Required]
    public string SerialNumber { get; init; } = string.Empty;

    public DateTime ValidFromUtc { get; init; }

    public DateTime ValidToUtc { get; init; }

    [Required]
    public string Algorithm { get; init; } = string.Empty;

    [Required]
    public string SourceType { get; init; } = string.Empty;

    public string? TokenSerialNumber { get; init; }
}
