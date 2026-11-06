using BillingService.Dtos;

namespace BillingService.Services;

public interface IFacturaService
{
    Task<IEnumerable<FacturaResponseDto>> ObtenerTodasAsync();
    Task<FacturaResponseDto?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<FacturaResponseDto>> CrearAsync(FacturaCreateDto dto);
}