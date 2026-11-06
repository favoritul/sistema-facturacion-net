using System.ComponentModel.DataAnnotations;

namespace BillingService.Dtos;

// ── ENTRADA ──

public class FacturaCreateDto
{
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public int? ClienteId { get; set; }

    [Required(ErrorMessage = "La factura debe tener al menos una línea")]
    [MinLength(1, ErrorMessage = "La factura debe tener al menos una línea")]
    public List<LineaFacturaCreateDto> Lineas { get; set; } = new();
}

public class LineaFacturaCreateDto
{
    [Required(ErrorMessage = "El producto es obligatorio")]
    public int? ProductoId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int? Cantidad { get; set; }
}

// ── SALIDA ──

public record FacturaResponseDto(
    int Id,
    int ClienteId,
    string ClienteNombre,
    DateTime FechaEmision,
    string Estado,
    decimal Total,
    List<LineaFacturaResponseDto> Lineas
);

public record LineaFacturaResponseDto(
    int Id,
    int ProductoId,
    string ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario
)
{
    public decimal Subtotal => Cantidad * PrecioUnitario;
}