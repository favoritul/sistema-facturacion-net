namespace BillingService.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nif { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}