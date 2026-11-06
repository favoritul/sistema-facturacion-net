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
public class ClientesController : ControllerBase
{
    private readonly BillingDbContext _context;

    public ClientesController(BillingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> GetClientes()
    {
        var clientes = await _context.Clientes
            .AsNoTracking()
            .Select(c => new ClienteResponseDto(c.Id, c.Nombre, c.Nif, c.Email))
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponseDto>> GetCliente(int id)
    {
        var cliente = await _context.Clientes
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClienteResponseDto(c.Id, c.Nombre, c.Nif, c.Email))
            .FirstOrDefaultAsync();

        if (cliente is null)
            return NotFound();

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponseDto>> CrearCliente(ClienteCreateDto dto)
    {
        var nifYaExiste = await _context.Clientes
            .AnyAsync(c => c.Nif == dto.Nif);

        if (nifYaExiste)
            return Conflict($"Ya existe un cliente con el NIF {dto.Nif}");

        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Nif = dto.Nif,
            Email = dto.Email
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        var respuesta = new ClienteResponseDto(
            cliente.Id, cliente.Nombre, cliente.Nif, cliente.Email);

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, respuesta);
    }
}