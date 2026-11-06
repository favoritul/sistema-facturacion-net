using BillingService.Dtos;
using BillingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IFacturaService _facturaService;

    public FacturasController(IFacturaService facturaService)
    {
        _facturaService = facturaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FacturaResponseDto>>> GetFacturas()
    {
        var facturas = await _facturaService.ObtenerTodasAsync();
        return Ok(facturas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FacturaResponseDto>> GetFactura(int id)
    {
        var factura = await _facturaService.ObtenerPorIdAsync(id);

        if (factura is null)
            return NotFound();

        return Ok(factura);
    }

    [HttpPost]
    public async Task<ActionResult<FacturaResponseDto>> CrearFactura(FacturaCreateDto dto)
    {
        var resultado = await _facturaService.CrearAsync(dto);

        if (!resultado.EsExitoso)
        {
            return resultado.TipoDeError switch
            {
                TipoError.NoEncontrado   => NotFound(resultado.Error),
                TipoError.ReglaDeNegocio => Conflict(resultado.Error),
                TipoError.AccesoDenegado => Forbid(),
                _                        => BadRequest(resultado.Error)
            };
        }

        return CreatedAtAction(
            nameof(GetFactura),
            new { id = resultado.Valor!.Id },
            resultado.Valor);
    }
}