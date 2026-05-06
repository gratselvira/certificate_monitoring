using System.ComponentModel.DataAnnotations;

namespace CertMonitor.Server.Models.Dto;

public sealed class ScanResultRequest
{
    [Required]
    public WorkstationDto Workstation { get; init; } = new();

    [Required]
    public AgentDto Agent { get; init; } = new();

    [Required]
    public ScanSessionDto ScanSession { get; init; } = new();

    public List<TokenDto> Tokens { get; init; } = new();

    public List<CertificateDto> Certificates { get; init; } = new();
}
