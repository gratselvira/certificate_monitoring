using System.ComponentModel.DataAnnotations;

namespace CertMonitor.Server.Models.Dto;

public sealed class WorkstationDto
{
    [Required]
    public string DeviceUid { get; init; } = string.Empty;

    [Required]
    public string Hostname { get; init; } = string.Empty;
}
