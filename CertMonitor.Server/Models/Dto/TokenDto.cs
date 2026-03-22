using System.ComponentModel.DataAnnotations;

namespace CertMonitor.Server.Models.Dto;

public sealed class TokenDto
{
    [Required]
    public string SerialNumber { get; init; } = string.Empty;

    [Required]
    public string TokenType { get; init; } = string.Empty;

    [Required]
    public string Model { get; init; } = string.Empty;

    [Required]
    public string Manufacturer { get; init; } = string.Empty;

    [Required]
    public string Pkcs11SlotId { get; init; } = string.Empty;
}
