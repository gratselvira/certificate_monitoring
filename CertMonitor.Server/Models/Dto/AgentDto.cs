using System.ComponentModel.DataAnnotations;

namespace CertMonitor.Server.Models.Dto;

public sealed class AgentDto
{
    [Required]
    public string Version { get; init; } = string.Empty;
}
