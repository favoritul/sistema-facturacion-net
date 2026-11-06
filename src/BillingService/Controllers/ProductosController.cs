using BillingService.Data;
using BillingService.Dtos;
using BillingService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly BillingDbContext _context;

    public ProductosController(BillingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> GetProductos()
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .Select(p => new ProductoResponseDto(p.Id, p.Nombre, p.Precio, p.Stock))
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoResponseDto>> GetProducto(int id)
    {
        var producto = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductoResponseDto(p.Id, p.Nombre, p.Precio, p.Stock))
            .FirstOrDefaultAsync();

        if (producto is null)
            return NotFound();

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoResponseDto>> CrearProducto(ProductoCreateDto dto)
    {
        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        var respuesta = new ProductoResponseDto(
            producto.Id, producto.Nombre, producto.Precio, producto.Stock);

        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, respuesta);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarProducto(int id, ProductoUpdateDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
            return NotFound();

        producto.Nombre = dto.Nombre;
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
            return NotFound();

        // Evitamos borrar productos que ya están en facturas
        var tieneFacturas = await _context.LineasFactura
            .AnyAsync(l => l.ProductoId == id);

        if (tieneFacturas)
            return Conflict("No se puede eliminar un producto que ya está referenciado en facturas.");

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
