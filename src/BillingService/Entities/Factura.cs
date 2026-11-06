namespace BillingService.Entities;

public enum EstadoFactura
{
    Borrador,
    Emitida,
    Pagada,
    Vencida
}

public class Factura
{
    public int Id { get; set; }
    public DateTime FechaEmision { get; set; }
    public EstadoFactura Estado { get; set; }
    public decimal Total { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public ICollection<LineaFactura> Lineas { get; set; } = new List<LineaFactura>();
}