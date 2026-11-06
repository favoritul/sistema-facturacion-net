namespace AuthService.Entities;

public class Invitacion
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public int ClienteId { get; set; }

    // Hash SHA-256 del token, nunca el token en claro
    public string TokenHash { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }

    // Null mientras no se haya usado. Es lo que la hace de un solo uso.
    public DateTime? FechaUso { get; set; }
}