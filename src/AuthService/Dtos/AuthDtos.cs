using System.ComponentModel.DataAnnotations;

namespace AuthService.Dtos;

public class RegistroRequestDto
{
    [Required(ErrorMessage = "El token de invitación es obligatorio")]
    public string TokenInvitacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponseDto(
    string AccessToken,
    string TokenType,
    int ExpiraEnSegundos
);

public record UsuarioResponseDto(
    int Id,
    string Email,
    string Rol,
    int? ClienteId
);

public class InvitacionRequestDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cliente es obligatorio")]
    public int? ClienteId { get; set; }

    public int DiasValidez { get; set; } = 7;
}

public record InvitacionResponseDto(
    string Token,
    string Email,
    int ClienteId,
    DateTime FechaExpiracion
);