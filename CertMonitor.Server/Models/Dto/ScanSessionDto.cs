using System.ComponentModel.DataAnnotations;

namespace CertMonitor.Server.Models.Dto;

public sealed class ScanSessionDto
{
    [Required]
    public string SessionUid { get; init; } = string.Empty;

    public DateTime StartedAtUtc { get; init; }

    public DateTime FinishedAtUtc { get; init; }
}
