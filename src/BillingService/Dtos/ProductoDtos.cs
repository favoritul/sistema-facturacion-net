using System.ComponentModel.DataAnnotations;

namespace BillingService.Dtos;

public record ProductoResponseDto(int Id, string Nombre, decimal Precio, int Stock);

public class ProductoCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(typeof(decimal), "0.01", "9999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El precio debe ser mayor que 0")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0")]
    public int Stock { get; set; }
}

public class ProductoUpdateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(typeof(decimal), "0.01", "9999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El precio debe ser mayor que 0")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0")]
    public int Stock { get; set; }
}