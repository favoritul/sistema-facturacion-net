namespace AuthService.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    // Vínculo con el cliente de BillingService.
    // Null para usuarios internos (admin) que no son clientes.
    public int? ClienteId { get; set; }

    public DateTime FechaAlta { get; set; }
    public bool Activo { get; set; } = true;
}