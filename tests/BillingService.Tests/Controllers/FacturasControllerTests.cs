using BillingService.Controllers;
using BillingService.Dtos;
using BillingService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingService.Tests.Controllers;

public class FacturasControllerTests
{
    private readonly Mock<IFacturaService> _servicioMock;
    private readonly FacturasController _controller;

    public FacturasControllerTests()
    {
        _servicioMock = new Mock<IFacturaService>();
        _controller = new FacturasController(_servicioMock.Object);
    }

    private static FacturaResponseDto CrearDtoDeEjemplo(int id = 1) => new(
        id,
        ClienteId: 1,
        ClienteNombre: "Cliente de prueba",
        FechaEmision: new DateTime(2026, 10, 8),
        Estado: "Emitida",
        Total: 269.70m,
        Lineas: new List<LineaFacturaResponseDto>
        {
            new(1, ProductoId: 1, ProductoNombre: "Teclado mecánico",
                Cantidad: 3, PrecioUnitario: 89.90m)
        }
    );

    [Fact]
    public async Task GetFactura_CuandoExiste_DevuelveOkConLaFactura()
    {
        // ── ARRANGE ──
        var esperada = CrearDtoDeEjemplo(id: 42);

        _servicioMock
            .Setup(s => s.ObtenerPorIdAsync(42))
            .ReturnsAsync(esperada);

        // ── ACT ──
        var resultado = await _controller.GetFactura(42);

        // ── ASSERT ──
        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(esperada);
    }

    [Fact]
    public async Task GetFactura_CuandoNoExiste_DevuelveNotFound()
    {
        _servicioMock
            .Setup(s => s.ObtenerPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync((FacturaResponseDto?)null);

        var resultado = await _controller.GetFactura(999);

        resultado.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CrearFactura_CuandoElServicioFallaPorNoEncontrado_DevuelveNotFound()
    {
        var dto = new FacturaCreateDto
        {
            ClienteId = 999,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1 }
            }
        };

        _servicioMock
            .Setup(s => s.CrearAsync(It.IsAny<FacturaCreateDto>()))
            .ReturnsAsync(ResultadoOperacion<FacturaResponseDto>.Fallo(
                "No existe el cliente con id 999", TipoError.NoEncontrado));

        var resultado = await _controller.CrearFactura(dto);

        resultado.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CrearFactura_CuandoElServicioFallaPorReglaDeNegocio_DevuelveConflict()
    {
        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 999 }
            }
        };

        _servicioMock
            .Setup(s => s.CrearAsync(It.IsAny<FacturaCreateDto>()))
            .ReturnsAsync(ResultadoOperacion<FacturaResponseDto>.Fallo(
                "Stock insuficiente", TipoError.ReglaDeNegocio));

        var resultado = await _controller.CrearFactura(dto);

        resultado.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CrearFactura_CuandoEsExitoso_DevuelveCreatedAtAction()
    {
        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 3 }
            }
        };

        var creada = CrearDtoDeEjemplo(id: 7);

        _servicioMock
            .Setup(s => s.CrearAsync(It.IsAny<FacturaCreateDto>()))
            .ReturnsAsync(ResultadoOperacion<FacturaResponseDto>.Exito(creada));

        var resultado = await _controller.CrearFactura(dto);

        var created = resultado.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(FacturasController.GetFactura));
        created.RouteValues!["id"].Should().Be(7);
        created.Value.Should().Be(creada);
    }

    [Fact]
    public async Task CrearFactura_PasaElDtoAlServicioSinModificarlo()
    {
        var dto = new FacturaCreateDto
        {
            ClienteId = 5,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 2, Cantidad = 4 }
            }
        };

        _servicioMock
            .Setup(s => s.CrearAsync(It.IsAny<FacturaCreateDto>()))
            .ReturnsAsync(ResultadoOperacion<FacturaResponseDto>.Exito(CrearDtoDeEjemplo()));

        await _controller.CrearFactura(dto);

        // ── ASSERT ── esto es lo que solo un mock puede comprobar
        _servicioMock.Verify(
            s => s.CrearAsync(It.Is<FacturaCreateDto>(d =>
                d.ClienteId == 5 &&
                d.Lineas.Count == 1 &&
                d.Lineas[0].ProductoId == 2 &&
                d.Lineas[0].Cantidad == 4)),
            Times.Once);
    }
}