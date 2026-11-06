using BillingService.Data;
using BillingService.Dtos;
using BillingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class FacturaService : IFacturaService
{
    private readonly BillingDbContext _context;
     private readonly ICurrentUser _currentUser;

    public FacturaService(BillingDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<FacturaResponseDto>> ObtenerTodasAsync()
    {
        var consulta = _context.Facturas
            .AsNoTracking()
            .Include(f => f.Cliente)
            .Include(f => f.Lineas)
                .ThenInclude(l => l.Producto)
            .AsQueryable();

        // Un cliente solo ve las suyas; un admin las ve todas
        if (!_currentUser.EsAdmin)
        {
            var clienteId = _currentUser.ClienteId;

            if (clienteId is null)
                return [];

            consulta = consulta.Where(f => f.ClienteId == clienteId.Value);
        }

        return await consulta
            .Select(f => MapearADto(f))
            .ToListAsync();
    }

    public async Task<FacturaResponseDto?> ObtenerPorIdAsync(int id)
    {
        var factura = await _context.Facturas
            .AsNoTracking()
            .Include(f => f.Cliente)
            .Include(f => f.Lineas)
                .ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (factura is null)
            return null;

        // Comprobación de propiedad: evita el IDOR
        if (!_currentUser.EsAdmin && factura.ClienteId != _currentUser.ClienteId)
            return null;

        return MapearADto(factura);
    }

    public async Task<ResultadoOperacion<FacturaResponseDto>> CrearAsync(FacturaCreateDto dto)
    {

        // Un cliente solo puede facturar a su propio ClienteId
        if (!_currentUser.EsAdmin && dto.ClienteId!.Value != _currentUser.ClienteId)
            return ResultadoOperacion<FacturaResponseDto>.Fallo(
                "No puede crear facturas para otro cliente",
                TipoError.AccesoDenegado);

        // 1. Validar que el cliente existe
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == dto.ClienteId!.Value);

        if (cliente is null)
            return ResultadoOperacion<FacturaResponseDto>.Fallo(
                $"No existe el cliente con id {dto.ClienteId}",
                TipoError.NoEncontrado);

        // 2. Detectar productos duplicados en la petición
        var idsProductos = dto.Lineas.Select(l => l.ProductoId!.Value).ToList();

        if (idsProductos.Count != idsProductos.Distinct().Count())
            return ResultadoOperacion<FacturaResponseDto>.Fallo(
                "La factura contiene líneas duplicadas para el mismo producto",
                TipoError.ReglaDeNegocio);

        // 3. Cargar todos los productos en UNA sola consulta (evita N+1)
        var productos = await _context.Productos
            .Where(p => idsProductos.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // 4. Validar que todos existen
        var idsInexistentes = idsProductos.Where(id => !productos.ContainsKey(id)).ToList();

        if (idsInexistentes.Count != 0)
            return ResultadoOperacion<FacturaResponseDto>.Fallo(
                $"No existen los productos con id: {string.Join(", ", idsInexistentes)}",
                TipoError.NoEncontrado);

        // 5. Validar stock suficiente
        foreach (var linea in dto.Lineas)
        {
            var producto = productos[linea.ProductoId!.Value];
            if (producto.Stock < linea.Cantidad!.Value)
                return ResultadoOperacion<FacturaResponseDto>.Fallo(
                    $"Stock insuficiente para '{producto.Nombre}'. " +
                    $"Disponible: {producto.Stock}, solicitado: {linea.Cantidad}",
                    TipoError.ReglaDeNegocio);
        }

        // 6. Construir la factura con los precios REALES de la base de datos
        var factura = new Factura
        {
            ClienteId = cliente.Id,
            FechaEmision = DateTime.UtcNow,
            Estado = EstadoFactura.Emitida,
            Lineas = dto.Lineas.Select(l => new LineaFactura
            {
                ProductoId = l.ProductoId!.Value,
                Cantidad = l.Cantidad!.Value,
                PrecioUnitario = productos[l.ProductoId!.Value].Precio
            }).ToList()
        };

        factura.Total = factura.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);

        // 7. Descontar stock
        foreach (var linea in factura.Lineas)
            productos[linea.ProductoId].Stock -= linea.Cantidad;

        // 8. Guardar todo de forma atómica
        _context.Facturas.Add(factura);
        await _context.SaveChangesAsync();

        // 9. Recargar con las relaciones para devolver la respuesta completa
        var facturaCreada = await ObtenerPorIdAsync(factura.Id);

        return ResultadoOperacion<FacturaResponseDto>.Exito(facturaCreada!);
    }

    private static FacturaResponseDto MapearADto(Factura f) => new(
        f.Id,
        f.ClienteId,
        f.Cliente.Nombre,
        f.FechaEmision,
        f.Estado.ToString(),
        f.Total,
        f.Lineas.Select(l => new LineaFacturaResponseDto(
            l.Id,
            l.ProductoId,
            l.Producto.Nombre,
            l.Cantidad,
            l.PrecioUnitario
        )).ToList()
    );
}