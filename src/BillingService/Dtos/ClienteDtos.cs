using System.ComponentModel.DataAnnotations;

namespace BillingService.Dtos;

public record ClienteResponseDto(
    int Id,
    string Nombre,
    string Nif,
    string Email
);

public class ClienteCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIF es obligatorio")]
    [MaxLength(20)]
    public string Nif { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
}