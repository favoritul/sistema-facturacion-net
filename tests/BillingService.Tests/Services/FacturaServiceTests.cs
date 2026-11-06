using BillingService.Data;
using BillingService.Dtos;
using BillingService.Entities;
using BillingService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BillingService.Tests.Services;

public class FacturaServiceTests
{
    private static BillingDbContext CrearContextoEnMemoria()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BillingDbContext(options);
    }

    private static async Task SembrarClienteYProductoAsync(
        BillingDbContext context,
        int stock = 10,
        decimal precio = 89.90m)
    {
        context.Clientes.Add(new Cliente
        {
            Id = 1,
            Nombre = "Cliente de prueba",
            Nif = "B00000001",
            Email = "test@ejemplo.es"
        });

        context.Productos.Add(new Producto
        {
            Id = 1,
            Nombre = "Teclado mecánico",
            Precio = precio,
            Stock = stock
        });

        await context.SaveChangesAsync();
    }

    private static FacturaService CrearServicio(
        BillingDbContext context,
        bool esAdmin = true,
        int? clienteId = null)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.EstaAutenticado).Returns(true);
        currentUser.Setup(u => u.EsAdmin).Returns(esAdmin);
        currentUser.Setup(u => u.ClienteId).Returns(clienteId);

        return new FacturaService(context, currentUser.Object);
    }

    [Fact]
    public async Task CrearAsync_ConDatosValidos_CalculaTotalCorrectamente()
    {
        // ── ARRANGE ──
        await using var context = CrearContextoEnMemoria();

        await SembrarClienteYProductoAsync(context);

        var servicio = CrearServicio(context);

        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 3 }
            }
        };

        // ── ACT ──
        var resultado = await servicio.CrearAsync(dto);

        // ── ASSERT ──
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.Total.Should().Be(269.70m);
        resultado.Valor.Lineas.Should().HaveCount(1);
        resultado.Valor.Lineas[0].Subtotal.Should().Be(269.70m);
        resultado.Valor.Lineas[0].PrecioUnitario.Should().Be(89.90m);
    }

    [Fact]
    public async Task CrearAsync_ConClienteInexistente_DevuelveNoEncontrado()
    {
        // ── ARRANGE ──
        await using var context = CrearContextoEnMemoria();
        var servicio = CrearServicio(context);

        var dto = new FacturaCreateDto
        {
            ClienteId = 999,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1 }
            }
        };

        // ── ACT ──
        var resultado = await servicio.CrearAsync(dto);

        // ── ASSERT ──
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoDeError.Should().Be(TipoError.NoEncontrado);
    }
    
    [Fact]
    public async Task CrearAsync_ConProductoInexistente_DevuelveNoEncontrado()
    {
        // ── ARRANGE ──
        await using var context = CrearContextoEnMemoria();

        context.Clientes.Add(new Cliente
        {
            Id = 1,
            Nombre = "Cliente de prueba",
            Nif = "B00000001",
            Email = "test@ejemplo.es"
        });

        await context.SaveChangesAsync();

        var servicio = CrearServicio(context);

        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 999, Cantidad = 1 }
            }
        };

        // ── ACT ──
        var resultado = await servicio.CrearAsync(dto);

        // ── ASSERT ──
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoDeError.Should().Be(TipoError.NoEncontrado);
    }

    [Fact]
    public async Task CrearAsync_ConStockInsuficiente_DevuelveReglaDeNegocio()
    {
        // ── ARRANGE ──
        await using var context = CrearContextoEnMemoria();

        await SembrarClienteYProductoAsync(context, stock: 5); // Solo 5 unidades disponibles

        var servicio = CrearServicio(context);

        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 10 } // Pedimos 10, solo hay 5
            }
        };

        // ── ACT ──
        var resultado = await servicio.CrearAsync(dto);

        // ── ASSERT ──
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoDeError.Should().Be(TipoError.ReglaDeNegocio);
    }

    [Fact]
    public async Task CrearAsync_ConLineasDuplicadas_DevuelveReglaDeNegocio()
    {
        // ── ARRANGE ──
        await using var context = CrearContextoEnMemoria();

        context.Productos.Add(new Producto
        {
            Id = 2,
            Nombre = "Ratón",
            Precio = 25.50m,
            Stock = 8
        });
        await SembrarClienteYProductoAsync(context);

        var servicio = CrearServicio(context);

        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 2 },
                new() { ProductoId = 1, Cantidad = 3 }, // Producto duplicado,
                new() { ProductoId = 2, Cantidad = 2 }
            }
        };

        // ── ACT ──
        var resultado = await servicio.CrearAsync(dto);

        // ── ASSERT ──
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoDeError.Should().Be(TipoError.ReglaDeNegocio);
    }

    [Fact]
    public async Task CrearAsync_ConDatosValidos_DescuentaStock()
    {
        // ── ARRANGE ──
        await using var context = CrearContextoEnMemoria();

        await SembrarClienteYProductoAsync(context);

        var servicio = CrearServicio(context);

        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 3 } // Pedimos 3
            }
        };

        // ── ACT ──
        var resultado = await servicio.CrearAsync(dto);

        // ── ASSERT ──
        resultado.EsExitoso.Should().BeTrue();
        // Verificar que el stock se descontó correctamente
        var productoActualizado = await context.Productos.FindAsync(1);
        productoActualizado.Should().NotBeNull();
        productoActualizado!.Stock.Should().Be(7);
    }

    [Fact]
    public async Task CrearAsync_TrasCambiarPrecioDelProducto_LaFacturaMantieneElPrecioOriginal()
    {
        // ── ARRANGE ──
        await using var context = CrearContextoEnMemoria();

        await SembrarClienteYProductoAsync(context);

        var servicio = CrearServicio(context);

        var dto = new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = new List<LineaFacturaCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 2 } // Pedimos 2
            }
        };

        // ── ACT ──
        var resultado = await servicio.CrearAsync(dto);

        // ── ASSERT ──
        resultado.EsExitoso.Should().BeTrue();

        // 1. Cargar la entidad (queda rastreada por el Change Tracker)
        var producto = await context.Productos.FindAsync(1);
        var facturaId = resultado.Valor!.Id;

        // 2. Modificar la propiedad
        producto!.Precio = 150m;

        // 3. Persistir el cambio
        await context.SaveChangesAsync();
    
        var facturaRecuperada = await servicio.ObtenerPorIdAsync(facturaId);
        facturaRecuperada.Should().NotBeNull();
        facturaRecuperada!.Lineas[0].PrecioUnitario.Should().Be(89.90m);
        facturaRecuperada!.Total.Should().Be(179.80m);
    }

    [Fact]
    public async Task ObtenerTodasAsync_ComoCliente_SoloDevuelveSusFacturas()
    {
        // ── ARRANGE ── dos clientes con una factura cada uno
        await using var context = CrearContextoEnMemoria();
        await SembrarClienteYProductoAsync(context);

        context.Clientes.Add(new Cliente
        {
            Id = 2,
            Nombre = "Otro cliente",
            Nif = "B00000002",
            Email = "otro@ejemplo.es"
        });
        await context.SaveChangesAsync();

        var admin = CrearServicio(context);

        await admin.CrearAsync(new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = [new() { ProductoId = 1, Cantidad = 1 }]
        });

        await admin.CrearAsync(new FacturaCreateDto
        {
            ClienteId = 2,
            Lineas = [new() { ProductoId = 1, Cantidad = 1 }]
        });

        // ── ACT ── ahora pregunta el cliente 1
        var servicioCliente = CrearServicio(context, esAdmin: false, clienteId: 1);
        var facturas = await servicioCliente.ObtenerTodasAsync();

        // ── ASSERT ──
        facturas.Should().HaveCount(1);
        facturas.First().ClienteId.Should().Be(1);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConFacturaDeOtroCliente_DevuelveNull()
    {
        await using var context = CrearContextoEnMemoria();
        await SembrarClienteYProductoAsync(context);

        var admin = CrearServicio(context);
        var creada = await admin.CrearAsync(new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = [new() { ProductoId = 1, Cantidad = 1 }]
        });

        // El cliente 2 intenta leer la factura del cliente 1
        var intruso = CrearServicio(context, esAdmin: false, clienteId: 2);
        var resultado = await intruso.ObtenerPorIdAsync(creada.Valor!.Id);

        // Null, no excepción: indistinguible de "no existe"
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerTodasAsync_SinClienteAsociado_DevuelveListaVacia()
    {
        await using var context = CrearContextoEnMemoria();
        await SembrarClienteYProductoAsync(context);

        var admin = CrearServicio(context);
        await admin.CrearAsync(new FacturaCreateDto
        {
            ClienteId = 1,
            Lineas = [new() { ProductoId = 1, Cantidad = 1 }]
        });

        // Usuario autenticado pero sin clienteId en el token
        var sinCliente = CrearServicio(context, esAdmin: false, clienteId: null);
        var facturas = await sinCliente.ObtenerTodasAsync();

        facturas.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearAsync_ComoClienteParaOtroCliente_DevuelveAccesoDenegado()
    {
        await using var context = CrearContextoEnMemoria();
        await SembrarClienteYProductoAsync(context);

        context.Clientes.Add(new Cliente
        {
            Id = 2,
            Nombre = "Otro cliente",
            Nif = "B00000002",
            Email = "otro@ejemplo.es"
        });
        await context.SaveChangesAsync();

        // El cliente 1 intenta facturar a nombre del cliente 2
        var servicio = CrearServicio(context, esAdmin: false, clienteId: 1);

        var resultado = await servicio.CrearAsync(new FacturaCreateDto
        {
            ClienteId = 2,
            Lineas = [new() { ProductoId = 1, Cantidad = 1 }]
        });

        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoDeError.Should().Be(TipoError.AccesoDenegado);
    }
}