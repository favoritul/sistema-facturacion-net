namespace BillingService.Entities;

public class LineaFactura
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
}